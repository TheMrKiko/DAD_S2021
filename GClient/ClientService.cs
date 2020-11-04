using Grpc.Core;
using System.Threading.Tasks;

namespace GC
{
    public class ClientService : ChatClientService.ChatClientServiceBase {
        IChatClientService clientLogic;

        public ClientService(IChatClientService clientLogic) {
            this.clientLogic = clientLogic;
        }

        public override Task<RecvMsgReply> RecvMsg(
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
