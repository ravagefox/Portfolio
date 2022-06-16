// Source: DeferredObject
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

using Wargame.Data.Gos.Components;

namespace Wargame.Data.Gos
{
    public class DeferredObject : WorldObject
    {

        public bool IsShadowCaster
        {
            get => this.GetComponent<ModelRenderer>().IsShadowCaster;
            set => this.GetComponent<ModelRenderer>().IsShadowCaster = value;
        }
        public bool CanReceiveShadows
        {
            get => this.GetComponent<ModelRenderer>().CanReceiveShadows;
            set => this.GetComponent<ModelRenderer>().CanReceiveShadows = value;
        }


        public DeferredObject() : base()
        {
            this.Transform.UseUniformScale = true;
            this.Transform.SetUniformScale(1.0f);
        }

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}
