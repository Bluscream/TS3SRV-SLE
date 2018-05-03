using System;
using System.Threading.Tasks;
using NLog;
using TS3SRV_SLE.Internal;
using System.Timers;

namespace TS3SRV_SLE.Network
{
    public class PayloadProcessor
    {
        private ConnectionHandler CHandler;

        private static readonly Logger Logger = LogManager.GetLogger(Properties.TS3SRV_LOGGER_NAME);

        private byte[] TS3SRV_WEBLIST_AUTHKEY;
        private ushort TS3SRV_WEBLIST_SEQUENCEID;

        private readonly ServerProperties _serverProperties;

        private readonly Timer _timer = new Timer(600000);

        private bool awaitingAuthKey = false;

        public PayloadProcessor(ServerProperties serverProperties)
        {
            CHandler = new ConnectionHandler(serverProperties) { HandleIncoming = OnIncoming };
            CHandler.InitializeConnection();
            _serverProperties = serverProperties;
            TS3SRV_WEBLIST_AUTHKEY = new byte[4];
            TS3SRV_WEBLIST_SEQUENCEID = 1;
            _timer.Elapsed += TimerElapsed;
            TimerElapsed(null, null);
            _timer.Start();
        }

        private async void TimerElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            await ProcessInitialKeyRequest();
            while (awaitingAuthKey)
            {
                await Task.Delay(25);
            }
            var outgoingPayload = new OutgoingPayloadHandler(_serverProperties)
            {
                SequenceId = TS3SRV_WEBLIST_SEQUENCEID,
                AuthKey = TS3SRV_WEBLIST_AUTHKEY,
            };
            outgoingPayload.ConstructHeader();
            if (outgoingPayload.ConstructDataPacket())
            {
                await CHandler.SendPayloadAsync(outgoingPayload.RawPayload);
                Logger.Log(LogLevel.Info, "Sent packet with info to serverlist");
                Console.WriteLine("Sent packet to the serverlist");
            }
        }

        private void OnIncoming(IncomingPayloadHandler payload)
        {
            payload = ProcessIncomingPayload(payload);

            //TODO
        }


        #region "Begin LowLevel Payload Handling Routines"

        public async Task ProcessInitialKeyRequest()
        {
            awaitingAuthKey = true;
            OutgoingPayloadHandler Payload = new OutgoingPayloadHandler()
            {
                SequenceId = TS3SRV_WEBLIST_SEQUENCEID,
                PayloadType = PayloadTypes.Keyrequest,
            };
            Payload.ConstructHeader();
            await CHandler.SendPayloadAsync(Payload.Header);
            TS3SRV_WEBLIST_SEQUENCEID++;
        }


        private IncomingPayloadHandler ProcessIncomingPayload(IncomingPayloadHandler payload)
        {
            payload.AuthKey = new byte[4];
            payload.Header = new byte[4];

            //Payload Contains Authkey, save it
            if (payload.RawPayload.Length == (Properties.TS3SRV_WEBLIST_PROTOCOL_HEADERLEN + Properties.TS3SRV_WEBLIST_PROTOCOL_AUTHKEYLEN))
            {
                Array.Copy(payload.RawPayload, Properties.TS3SRV_WEBLIST_PROTOCOL_HEADERLEN, payload.AuthKey, 0, payload.RawPayload.Length - Properties.TS3SRV_WEBLIST_PROTOCOL_HEADERLEN);
                TS3SRV_WEBLIST_AUTHKEY = payload.AuthKey;
                awaitingAuthKey = false;
                Logger.Log(LogLevel.Info, "Got Authkey, saving it. ");
                Console.WriteLine("GOT AUTH KEY");
            }

            Array.Copy(payload.RawPayload, 0, payload.Header, 0, Properties.TS3SRV_WEBLIST_PROTOCOL_HEADERLEN);

            if (payload.RawPayload.Length == Properties.TS3SRV_WEBLIST_PROTOCOL_HEADERLEN + 1)
            {
                var responseCode = (ResponseStatus)payload.RawPayload[payload.RawPayload.Length - 1];
                Console.WriteLine("Got response: " + responseCode);
                if (responseCode.Equals(ResponseStatus.Ok))
                {
                    //Logger.Log(LogLevel.Info, "Successfully added server to the serverlist: " + responseCode);
                    Console.WriteLine("Successfully added server to the serverlist: " + responseCode);
                    TS3SRV_WEBLIST_SEQUENCEID++;
                    _timer.Interval = 600000;
                    _timer.Stop();
                    _timer.Start();
                } else if (responseCode.Equals(ResponseStatus.Busy))
                {
                    _timer.Interval = 60000;
                    _timer.Stop();
                    _timer.Start();
                }
            }

            //Ignore first header byte as it's not needed
            payload.SequenceId = BitConverter.ToUInt16(new byte[2] { payload.Header[2], payload.Header[1] }, 0);
            payload.PType = payload.Header[3];

            TS3SRV_WEBLIST_SEQUENCEID = payload.SequenceId;
            return payload;
        }

        #endregion
    }
}
