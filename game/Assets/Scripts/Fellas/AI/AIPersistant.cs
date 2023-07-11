using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class AIPersistant : AIBasic
    {
        [SerializeField]
        private int _parriesNeededToStop = 2;
        
        protected override void GotParriedEffect()
        {
            if(_attacksInRow > _parriesNeededToStop)
                base.GotParriedEffect();
        }
    }
}
