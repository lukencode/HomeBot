using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NCalc;

namespace HomeBot.Core.Command.Respond.BuiltIn
{
    public class CalculatorResponder : IMessageHandler
    {
        public bool Handle(Communication.ChatMessage message, Communication.ICommunicator comm)
        {
            var lowered = message.Message.ToLower();

            if (lowered.StartsWith("calc") || lowered.StartsWith("math"))
            {
                var trimmed = getExpression(lowered);
                var expression = new Expression(trimmed, EvaluateOptions.IgnoreCase);
                object result = null;

                try
                {
                    result = expression.Evaluate();
                }
                catch (Exception ex)
                {
                    if (ex is ArgumentException || ex is EvaluationException)
                        comm.SendMessage(message.From, "I couldn't evaluate your expression");
                }

                if (result == null)
                    return false;

                comm.SendMessage(message.From, string.Format("{0} = {1}", trimmed, result));

                return true;
            }

            return false;
        }

        private string getExpression(string msg)
        {
            var expression = msg.Replace("calc ", "").Replace("math ", "").Trim();
            return expression;
        }
    }
}
