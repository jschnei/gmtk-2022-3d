using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMachine
{
    Timer timer;
    Dictionary<string, float> durations;
    Dictionary<string, Dictionary<string, string>> transitions;

    public string curState = "";

    public StateMachine() {
        timer = new Timer(0);
        durations = new Dictionary<string, float>();
        transitions = new Dictionary<string, Dictionary<string, string>>();
    }

    public void AddState(string name, float duration) {
        durations[name] = duration;
        transitions[name] = new Dictionary<string, string>();
    }

    public void AddTransition(string source, string label, string target) {
        transitions[source][label] = target;
    }

    public void UpdateTimer(float deltaTime) {
        timer.UpdateTimer(deltaTime);
    }

    public void SetState(string state) {
        curState = state;
        timer = new Timer(durations[state]);
    }

    public bool IsReady() {
        return timer.IsOver();
    }

    public void Transition(string label) {
        if (timer.IsOver()) {
            if (transitions[curState].ContainsKey(label)) {
                SetState(transitions[curState][label]);
            }
        }
    }
}