using Grpc.Core;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetService : PuppetMasterService.PuppetMasterServiceBase {
        IPuppetMasterGUI clientLogic;

        public PuppetService(IPuppetMasterGUI clientLogic) {
            this.clientLogic = clientLogic;
        }

        /*public override Task<RecvMsgReply> RecvMsg(
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
        }*/
    }
}
