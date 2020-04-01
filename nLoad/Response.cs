using System;

namespace nLoad
{
    public class Response
    {
        public object Data { get; set; }
        public Exception Exception { get; set; }
        public string FailReason { get; set; }

        public static Response Ok(object data) => new Response { Data = data };
        public static Response Fail(Exception e) => new Response { Exception = e };
        public static Response Fail(string reason) => new Response { FailReason = reason };
    }
}
