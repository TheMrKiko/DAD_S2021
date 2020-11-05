using Grpc.Core;
using System.Threading.Tasks;

namespace GC
{
    public class GClientService : GCService.GCServiceBase {
        IClientGUI clientLogic;

        public GClientService(IClientGUI clientLogic) {
            this.clientLogic = clientLogic;
        }

        public Task<RecvMsgReply> RecvMsg(
            RecvMsgRequest request, ServerCallContext context) {
            return Task.FromResult(UpdateGUIwithMsg(request));
        }

        public RecvMsgReply UpdateGUIwithMsg(RecvMsgRequest request) {
           if ( clientLogic.AddMsgtoGUI(request.Msg)) { 
            return new RecvMsgReply
            {
                Ok = true
            };
            } else {
                return new RecvMsgReply
                {
                    Ok = false
                };

            }
        }
    }
}
