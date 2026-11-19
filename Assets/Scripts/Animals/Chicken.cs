using System.Collections;
using UnityEngine;

// A chicken that lives in its pen: hops to a random spot, stands around, pecks, and
// scurries off when the player gets close. Movement is done in code; the Animator
// only plays the "Jump" (hop), "Idle" and "Attack" (peck) states by name.
public class Chicken : MonoBehaviour
{
    [SerializeField] private Animator anim;

    [Header("Movement")]
    [SerializeField] private float hopSpeed = 0.9f;
    [SerializeField] private float fleeSpeed = 2.2f;
    [SerializeField] private float turnSpeed = 300f;

    [Header("Behaviour")]
    [SerializeField] private float fleeDistance = 1.8f;
    [SerializeField] private Vector2 idleTime = new Vector2(1.5f, 4f);
    [SerializeField, Range(0f, 1f)] private float peckChance = 0.5f;
    [SerializeField] private float peckDuration = 1f;

    [Header("Waddle")]
    [Tooltip("How far it rocks side to side while moving, in degrees.")]
    [SerializeField] private float waddleAngle = 9f;
    [Tooltip("Rocking speed. ~6 = one left-right rock every two hops of the jump animation.")]
    [SerializeField] private float waddleSpeed = 6f;

    private ChickenCoop coop;
    private Transform player;
    private string playing;

    // The script owns the root rotation: heading (which way it faces) plus the waddle roll.
    private Quaternion heading;
    private bool moving;
    private float waddlePhase;
    private float waddleWeight;

    public void Init(ChickenCoop owner) => coop = owner;

    private void Awake()
    {
        if (anim == null) anim = GetComponentInChildren<Animator>();
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) player = p.transform;
        heading = transform.rotation;
    }

    // Rocks around the feet (the root pivot). Eases in and out so it doesn't snap upright.
    private void LateUpdate()
    {
        waddleWeight = Mathf.MoveTowards(waddleWeight, moving ? 1f : 0f, Time.deltaTime * 5f);
        float speed = anim != null ? anim.speed : 1f;
        waddlePhase += Time.deltaTime * waddleSpeed * speed;

        float roll = Mathf.Sin(waddlePhase) * waddleAngle * waddleWeight;
        transform.rotation = heading * Quaternion.Euler(0f, 0f, roll);
    }

    private void OnEnable() => StartCoroutine(Live());

    private IEnumerator Live()
    {
        yield return null;   // the coop calls Init right after spawning us

        while (coop != null)
        {
            if (TooClose(out Vector3 away))
            {
                yield return MoveTo(coop.ClampToPen(transform.position + away * 2.5f), fleeSpeed);
                continue;
            }

            yield return MoveTo(coop.RandomPointInPen(), hopSpeed);

            Play("Idle");
            yield return Wait(Random.Range(idleTime.x, idleTime.y));

            if (Random.value < peckChance && !TooClose(out _))
            {
                Play("Attack");
                yield return Wait(peckDuration);
            }
        }
    }

    private IEnumerator MoveTo(Vector3 target, float speed)
    {
        target.y = transform.position.y;
        bool fleeing = speed > hopSpeed;

        Play("Jump");
        if (anim != null) anim.speed = fleeing ? 1.6f : 1f;
        moving = true;

        while ((target - transform.position).sqrMagnitude > 0.01f)
        {
            if (!fleeing && TooClose(out _)) break;   // stop wandering and run instead

            Vector3 dir = (target - transform.position).normalized;
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            heading = Quaternion.RotateTowards(heading, look, turnSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);
            yield return null;
        }

        moving = false;
        if (anim != null) anim.speed = 1f;
    }

    // Waits, but gives up early if the player walks up, so the chicken can flee.
    private IEnumerator Wait(float seconds)
    {
        for (float t = 0f; t < seconds; t += Time.deltaTime)
        {
            if (TooClose(out _)) yield break;
            yield return null;
        }
    }

    private bool TooClose(out Vector3 away)
    {
        away = Vector3.zero;
        if (player == null) return false;

        Vector3 d = transform.position - player.position;
        d.y = 0f;
        if (d.sqrMagnitude > fleeDistance * fleeDistance) return false;

        away = d.sqrMagnitude > 0.001f ? d.normalized : transform.forward;
        return true;
    }

    private void Play(string state)
    {
        if (anim == null || playing == state) return;
        playing = state;
        anim.CrossFade(state, 0.1f);
    }
}
