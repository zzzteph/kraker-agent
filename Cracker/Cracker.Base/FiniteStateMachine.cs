using System;
using System.Threading.Tasks;

namespace Cracker.Base
{
    public class FiniteStateMachine
    {
        private Func<Task> activeStateAction;

        public FiniteStateMachine(Func<Task> initialAction)
        {
            activeStateAction = initialAction;
        }

        public void SetStateAction(Func<Task> newAction) => activeStateAction = newAction;

        public Task RunAction() => activeStateAction.Invoke();
    }
}