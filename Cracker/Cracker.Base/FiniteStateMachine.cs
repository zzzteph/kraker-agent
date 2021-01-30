using System;

namespace Cracker.Base
{
    public class FiniteStateMachine
    {
        private Action activeStateAction;

        public FiniteStateMachine(Action initialAction)
        {
            activeStateAction = initialAction;
        }

        public void SetStateAction(Action newAction)
        {
            activeStateAction = newAction;
        }

        public void RunAction()
        {
            activeStateAction?.Invoke();
        }
    }
}