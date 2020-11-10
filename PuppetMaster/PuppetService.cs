using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetService : PMasterService.PMasterServiceBase {
        IPuppetMasterGUI clientLogic;

        public PuppetService(IPuppetMasterGUI clientLogic) {
            this.clientLogic = clientLogic;
        }

        public override Task<RegisterReply> Register(RegisterRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Master ---");
            Console.WriteLine("Some node just registed.");
            this.clientLogic.Register(request.Id, request.Type);
            return Task.FromResult(new RegisterReply());
        }

        /*public override Task<GetPartitionsReply> GetPartitionsInfo(GetPartitionsRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Master ---");
            Console.WriteLine("Some node asked to " + context.Method);
            return Task.FromResult(this.clientLogic.PartitionsInfo(request));
        }

        public override Task<GetServersInfoReply> GetServersInfo(GetServersInfoRequest request, ServerCallContext context)
        {
            Console.WriteLine();
            Console.WriteLine("--- Master ---");
            Console.WriteLine("Some node asked to " + context.Method);
            return Task.FromResult(this.clientLogic.ServersInfo(request));
        }*/

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
