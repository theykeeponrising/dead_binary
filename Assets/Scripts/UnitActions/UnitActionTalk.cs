using UnityEngine;

public class UnitActionTalk : UnitAction
{
    public override void UseAction()
    {
        // For testing purposes only
        // Unit will talk

        if (Unit.IsActing())
            return;

        SetPerformed(true);
        Unit.PlayerAction.SelectAction();

        string[] randomDialog = new string[]{
            "Well, things can't get much worse, right?",
            "Patrolling the Mojave almost makes you wish for a nuclear winter.",
            "So many roads, but no cars... what's that about?",
            "Damn.",
            "It's my turn to use the rocket launcher.",
            "I'm not really sure how these \"Healnades\" work. The future is great!"
        };

        Unit.Say(randomDialog[Random.Range(0, randomDialog.Length)]);
    }
}
