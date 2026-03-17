using UnityEngine;
using Ghosts.Emotions;
using System.Collections;

[CreateAssetMenu(menuName = "Pressure/MirrorFlashEffect")]
public class MirrorFlashEffect : PressureEffect
{
    [Header("Timing")]
    [SerializeField] private float stareTimeToAttack = 3f;
    [SerializeField] private float lostSightGraceTime = 0.15f;
    [SerializeField] private float maxLifetime = 6f;

    [Header("Punish")]
    [SerializeField] private float aggressionSpike = 75f;
    [SerializeField] private float sanityDamage = 15f;
    [SerializeField] private float attackCommitSeconds = 1.25f;

    [Header("Attack Staging")]
    [SerializeField] private float emergeDelay = 2f;
    [SerializeField] private float appearOffsetForward = 0.5f;
    [SerializeField] private float appearOffsetUp = 0f;

    [Header("Audio")]
    [SerializeField] private AudioClip appearClip;
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip punishClip;

    public override void Play(PressureDirector director, float intensity)
    {
        Collider mirrorCol = director.GetVisibleMirror();
        if (mirrorCol == null)
            return;

        GhostMirror ghostMirror = mirrorCol.GetComponentInParent<GhostMirror>();
        if (ghostMirror == null)
            return;

        GameObject ghostObject = ghostMirror.GhostObject;
        if (ghostObject.activeSelf)
            return;

        director.StartCoroutine(RunEncounter(director, mirrorCol, ghostMirror, intensity));
    }

    private IEnumerator RunEncounter(
        PressureDirector director,
        Collider targetMirror,
        GhostMirror ghostMirror,
        float intensity)
    {
        GameObject ghostObject = ghostMirror.GhostObject;
        ghostObject.SetActive(true);

        if (appearClip != null)
        {
            director.Play3DAudio(
                appearClip,
                director.controller.player.position,
                Mathf.Lerp(0.35f, 0.8f, intensity)
            );
        }

        float stareTimer = 0f;
        float unseenTimer = 0f;
        float lifeTimer = 0f;
        bool warned = false;

        while (ghostObject.activeSelf && lifeTimer < maxLifetime)
        {
            lifeTimer += Time.deltaTime;

            Collider currentMirror = director.GetVisibleMirror();
            bool stillLookingAtSameMirror = currentMirror == targetMirror;

            if (stillLookingAtSameMirror)
            {
                unseenTimer = 0f;
                stareTimer += Time.deltaTime;

                if (!warned && stareTimer >= stareTimeToAttack * 0.5f)
                {
                    warned = true;

                    if (warningClip != null)
                    {
                        director.Play3DAudio(
                            warningClip,
                            director.controller.player.position,
                            Mathf.Lerp(0.25f, 0.7f, intensity)
                        );
                    }
                }

                if (stareTimer >= stareTimeToAttack)
                {
                    yield return TriggerPunish(director, ghostObject, intensity);
                    break;
                }
            }
            else
            {
                unseenTimer += Time.deltaTime;

                if (unseenTimer >= lostSightGraceTime)
                    break;
            }

            yield return null;
        }

        ghostObject.SetActive(false);
    }

    private IEnumerator TriggerPunish(PressureDirector director, GameObject ghostObject, float intensity)
    {
        Vector3 mirrorPos = ghostObject.transform.position;
        Vector3 mirrorForward = ghostObject.transform.forward;

        // 1) Play sting first while she's still "in" the mirror
        if (punishClip != null)
        {
            director.Play3DAudio(
                punishClip,
                director.controller.player.position,
                Mathf.Lerp(0.5f, 1.5f, intensity)
            );
        }

        // 2) Hide mirror apparition
        ghostObject.SetActive(false);

        // 3) Apply mental punish immediately
        director.controller.context.insanitySystem.ModifyInsanity(
            -Mathf.Lerp(sanityDamage * 0.5f, sanityDamage, intensity)
        );

        director.controller.context.emotion.AddFromAI(
            EmotionType.Aggression,
            aggressionSpike,
            1f
        );

        // 4) Small delay so the audio/visual beat lands
        yield return new WaitForSeconds(emergeDelay);

        // 5) Move Mom's real body to the mirror as if emerging from it
        Vector3 emergePos =
            mirrorPos +
            mirrorForward * appearOffsetForward +
            Vector3.up * appearOffsetUp;

        if (director.controller.agent != null && director.controller.agent.enabled)
        {
            director.controller.agent.Warp(emergePos);
        }
        else
        {
            director.controller.transform.position = emergePos;
        }

        // face the player immediately
        Vector3 toPlayer = director.controller.player.position - director.controller.transform.position;
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude > 0.001f)
        {
            director.controller.transform.rotation = Quaternion.LookRotation(toPlayer);
        }

        // 6) Now commit to chase
        director.QueueAttackCommit(attackCommitSeconds);
    }
}