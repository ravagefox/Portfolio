// Source: GameStateManager
/* 
   ---------------------------------------------------------------
                        CREXIUM PTY LTD
   ---------------------------------------------------------------

     The software is provided 'AS IS', without warranty of any kind,
   express or implied, including but not limited to the warrenties
   of merchantability, fitness for a particular purpose and
   noninfringement. In no event shall the authors or copyright
   holders be liable for any claim, damages, or other liability,
   whether in an action of contract, tort, or otherwise, arising
   from, out of or in connection with the software or the use of
   other dealings in the software.
*/

namespace Wargame.Data.IO
{
    public sealed class GameStateManager
    {
        public GameState CurrentState { get; private set; }

        public GameState PreviousState { get; private set; }


        private GameState defaultState;


        public GameStateManager(GameState defaultState)
        {
            this.defaultState = defaultState;
        }

        public GameState Restore()
        {
            return this.Switch(this.PreviousState);
        }

        public GameState RestoreToDefault()
        {
            return this.Switch(this.defaultState);
        }

        public GameState Switch(GameState nextState)
        {
            if (this.CurrentState != nextState)
            {
                this.PreviousState = this.CurrentState;
                this.CurrentState = nextState;
            }

            return this.CurrentState;
        }
    }
}
