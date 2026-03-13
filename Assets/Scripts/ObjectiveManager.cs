using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class ObjectiveManager : MonoBehaviour
{
    public static ObjectiveManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI checklistText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Header("Behavior")]
    [SerializeField] private bool startOnPlay = true;
    [SerializeField] private bool hideCompletedOptionalSteps = false;

    [Header("Objectives")]
    [SerializeField] private List<ObjectiveData> objectives = new List<ObjectiveData>();

    private int currentObjectiveIndex = -1;
    private readonly HashSet<string> completedStepIds = new HashSet<string>();
    private bool sequenceFinished;

    public ObjectiveData CurrentObjective
    {
        get
        {
            if (currentObjectiveIndex < 0 || currentObjectiveIndex >= objectives.Count)
                return null;

            return objectives[currentObjectiveIndex];
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        if (startOnPlay)
            BeginObjectiveSequence();
        else
            RefreshUI();
    }

    public void BeginObjectiveSequence()
    {
        if (objectives.Count == 0)
        {
            Debug.LogWarning("ObjectiveManager: No objectives assigned.");
            RefreshUI();
            return;
        }

        sequenceFinished = false;
        currentObjectiveIndex = 0;
        RefreshUI();
    }

    public void CompleteStep(string stepId)
    {
        if (string.IsNullOrWhiteSpace(stepId))
        {
            Debug.LogWarning("ObjectiveManager: Tried to complete an empty step id.");
            return;
        }

        if (sequenceFinished || CurrentObjective == null)
            return;

        ObjectiveStepData step = GetStepInCurrentObjective(stepId);
        if (step == null)
        {
            Debug.LogWarning($"ObjectiveManager: Step '{stepId}' is not part of the current objective '{CurrentObjective.objectiveId}'.");
            return;
        }

        if (completedStepIds.Contains(stepId))
            return;

        completedStepIds.Add(stepId);
        RefreshUI();

        if (AreRequiredStepsComplete(CurrentObjective))
        {
            AdvanceToNextObjective();
        }
    }

    public bool IsStepComplete(string stepId)
    {
        return completedStepIds.Contains(stepId);
    }

    public bool IsCurrentObjective(string objectiveId)
    {
        return CurrentObjective != null && CurrentObjective.objectiveId == objectiveId;
    }

    public void ForceAdvanceObjective()
    {
        if (sequenceFinished)
            return;

        AdvanceToNextObjective();
    }

    private void AdvanceToNextObjective()
    {
        currentObjectiveIndex++;

        if (currentObjectiveIndex >= objectives.Count)
        {
            sequenceFinished = true;
            currentObjectiveIndex = objectives.Count - 1;
            RefreshUI(true);
            return;
        }

        RefreshUI();
    }

    private ObjectiveStepData GetStepInCurrentObjective(string stepId)
    {
        if (CurrentObjective == null)
            return null;

        foreach (ObjectiveStepData step in CurrentObjective.steps)
        {
            if (step.stepId == stepId)
                return step;
        }

        return null;
    }

    private bool AreRequiredStepsComplete(ObjectiveData objective)
    {
        if (objective == null)
            return false;

        foreach (ObjectiveStepData step in objective.steps)
        {
            if (!step.required)
                continue;

            if (!completedStepIds.Contains(step.stepId))
                return false;
        }

        return true;
    }

    private void RefreshUI(bool allObjectivesComplete = false)
    {
        if (headerText != null)
            headerText.text = "OBJECTIVE";

        if (allObjectivesComplete)
        {
            if (titleText != null)
                titleText.text = "All objectives complete";

            if (descriptionText != null)
                descriptionText.text = "You completed the bank objective chain.";

            if (checklistText != null)
                checklistText.text = "<color=#7CFF7C>✓ Mission complete</color>";

            if (progressText != null)
                progressText.text = $"{objectives.Count}/{objectives.Count}";

            return;
        }

        if (CurrentObjective == null)
        {
            if (titleText != null) titleText.text = "No active objective";
            if (descriptionText != null) descriptionText.text = "";
            if (checklistText != null) checklistText.text = "";
            if (progressText != null) progressText.text = "0/0";
            return;
        }

        if (titleText != null)
            titleText.text = CurrentObjective.title;

        if (descriptionText != null)
            descriptionText.text = CurrentObjective.description;

        if (progressText != null)
            progressText.text = $"{currentObjectiveIndex + 1}/{objectives.Count}";

        if (checklistText != null)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < CurrentObjective.steps.Count; i++)
            {
                ObjectiveStepData step = CurrentObjective.steps[i];
                bool complete = completedStepIds.Contains(step.stepId);

                if (hideCompletedOptionalSteps && complete && !step.required)
                    continue;

                string icon = complete ? "✓" : "○";
                string color = complete ? "#7CFF7C" : "#FFFFFF";
                string text = complete
                    ? $"<s>{step.displayText}</s>"
                    : step.displayText;

                sb.AppendLine($"<color={color}>{icon} {text}</color>");
            }

            checklistText.text = sb.ToString().TrimEnd();
        }
    }
}

[Serializable]
public class ObjectiveData
{
    public string objectiveId;
    public string title;

    [TextArea(2, 4)]
    public string description;

    public List<ObjectiveStepData> steps = new List<ObjectiveStepData>();
}

[Serializable]
public class ObjectiveStepData
{
    public string stepId;
    public string displayText;
    public bool required = true;
}