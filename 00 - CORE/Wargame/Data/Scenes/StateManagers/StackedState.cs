// Source: StackedState
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

using System.Collections.Generic;
using Engine.Core;
using Wargame.Data.IO;

namespace Wargame.Data.Scenes.StateManagers
{
    public class StackedState : GameState
    {

        private List<GameState> states;


        public StackedState(
            string name,
            params GameState[] states) : base(name)
        {
            this.states = new List<GameState>(states);
        }

        public override void InitializeState()
        {
            this.states.ForEach(s => s.InitializeState());
        }

        public override void HandleInput()
        {
            this.states.ForEach(s => s.HandleInput());
        }

        public override void Apply(Time frameTime)
        {
            this.states.ForEach(s => s.Apply(frameTime));
        }

        public override void Update(Time frameTime)
        {
            this.states.ForEach(s => s.Update(frameTime));
        }
    }
}
