using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace PuppetMaster
{
    public class PuppetService : PMasterService.PMasterServiceBase {
        IPuppetMasterGUI clientLogic;
        private System.Threading.EventWaitHandle ewh = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.ManualReset);

        public PuppetService(IPuppetMasterGUI clientLogic) {
            this.clientLogic = clientLogic;
            Task.Run(async () =>  {
                Console.WriteLine("Start count");
                await Task.Delay(20000);
                Console.WriteLine("End count");
                ewh.Set();
            }
            );
        }

        public override Task<GetPartitionsReply> GetPartitionsInfo(GetPartitionsRequest request, ServerCallContext context)
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
            ewh.WaitOne();
            Console.WriteLine("bla");
            return Task.FromResult(this.clientLogic.ServersInfo(request));
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
