﻿namespace doob.SignalARRR.Common.Constants
{
    public class MethodNames
    {
        public static string InvokeMessageOnServer { get; } = "InvokeMessage";
        public static string InvokeMessageResultOnServer { get; } = "InvokeMessageResult";
        public static string SendMessageToServer { get; } = "SendMessage";
        public static string StreamMessageFromServer { get; } = "StreamMessage";

        public static string InvokeServerRequest { get; } = "InvokeServerRequest";
        public static string ReplyServerRequest { get; } = "ReplyServerRequest";

        public static string ChallengeAuthentication { get; } = "ChallengeAuthentication";
        public static string InvokeServerMessage { get; set; } = "InvokeServerMessage";

        public static string CancelTokenFromServer { get; set; } = "CancelTokenFromServer";
    }
}
