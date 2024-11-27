using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatorStateData", menuName = "ScriptableObjects/AnimatorStateData")]
public class AnimatorStateData : ScriptableObject
{
    public RuntimeAnimatorController animatorController;
    public List<string> stateNames;
    public List<int> stateHashes;
    public List<string> parameterNames;
    public List<int> parameterHashes;

    public IndexedDictionary<string, List<string>> groupedStateNamesDict = new();

#if UNITY_EDITOR
    private void OnValidate()
    {
        groupedStateNamesDict = new IndexedDictionary<string, List<string>>();
        stateNames.Clear();
        stateHashes.Clear();
        var controller = (UnityEditor.Animations.AnimatorController)animatorController;
        foreach (var layer in controller.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                stateNames.Add(state.state.name);
                stateHashes.Add(state.state.nameHash);
                //Debug.Log($"State: {state.state.name}, Hash: {state.state.nameHash}");

                // Group state names
                var clipName = state.state.name;
                var signal = clipName.LastIndexOf('_');
                if (signal == -1) continue;
                var strNum = clipName[(signal + 1)..];
                if (!int.TryParse(strNum, out var num) || num >= 10) continue;
                var strName = clipName[..signal];
                //Debug.Log($"{strName} : {num}");
                if (!groupedStateNamesDict.ContainsKey(strName))
                {
                    groupedStateNamesDict.Add(strName, new List<string>());
                    //Debug.Log($"Add {strName}");
                }

                if (!groupedStateNamesDict[strName].Contains(clipName))
                {
                    groupedStateNamesDict[strName].Add(clipName);
                    //Debug.Log($"Add {clipName} (child of {strName})");
                }
            }
        }

        parameterNames.Clear();
        parameterHashes.Clear();
        foreach (var parameter in controller.parameters)
        {
            parameterNames.Add(parameter.name);
            parameterHashes.Add(parameter.nameHash);
            //Debug.Log($"Parameter: {parameter.name}, Hash: {parameter.nameHash}");
        }
    }
#endif
}