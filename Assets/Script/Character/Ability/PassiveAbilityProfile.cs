using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PassiveAbility", menuName = "ScriptableObjects/PassiveAbility")]
public class PassiveAbilityProfile : AbilityProfile
{
    
}

[Serializable]
public class PassiveAbilityInstance : AbilityInstance
{
    public PassiveAbilityProfile passive;
}