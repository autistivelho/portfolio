using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class CombatViewController : MonoBehaviour
{
    public static CombatViewController Instance;

    public bool DrawCharacterPath;
    public float MinimumDrawDistance;
    public LineRenderer PathLine;
    public LineRenderer TrajectoryRenderer;
    public LineRenderer TargetCircle;
    public Transform TargetCircleTransform;

    public bool ValidDestination;

    private Camera cam;
    private PlayerControl playerControl;
    private PlayerMovement character;
    private NavMeshAgent agent;
    private CharacterCombatController characterCombatController;
    private Vector3 previousMousePosition;

    private readonly Gradient gradientValid = new Gradient();
    private readonly Gradient gradientInvalid = new Gradient();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.Log("Another Instance of CombatView was running");
        }
    }

    private void Start()
    {
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        playerControl = cam.GetComponent<PlayerControl>();
        gradientValid.SetKeys(new[] {new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f)},
            new[] {new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f)});
        gradientInvalid.SetKeys(new[] {new GradientColorKey(Color.red, 0.0f), new GradientColorKey(Color.red, 1.0f)},
            new[] {new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f)});
        DrawTargetCircle(Vector3.zero, true);
    }

    private void Update()
    {
        if (GlobalTBModeController.Instance.IsTurnBased) DrawCharacterPath = characterCombatController.MovementLeft > MinimumDrawDistance;
    }

    private void FixedUpdate()
    {
        var currentMousePosition = Input.mousePosition;

        if (currentMousePosition == previousMousePosition) return;
        
        PathLine.enabled = false;
        TargetCircle.enabled = false;
        TrajectoryRenderer.enabled = false;

        var eventSys = EventSystem.current;

        if (!characterCombatController || !characterCombatController.MyTurn && GlobalTBModeController.Instance.IsTurnBased || !characterCombatController.IsTargetingSkill && !DrawCharacterPath || eventSys.IsPointerOverGameObject()) return;

        Ray ray = cam.ScreenPointToRay(currentMousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100, ~LayerMask.GetMask("VisibilityColliders")))
        {
            previousMousePosition = currentMousePosition;

            if (characterCombatController.IsTargetingSkill)
            {
                TrajectoryRenderer.enabled = true;
                var skillTrajectoryStartPoint = character.transform.position + Vector3.up;

                var skillTrajectoryEndPoint = hit.point;

                if (hit.collider.gameObject.layer == 12)
                {
                    skillTrajectoryEndPoint += Vector3.up;
                }
                ValidDestination = Vector3.Distance(skillTrajectoryStartPoint, skillTrajectoryEndPoint) <= characterCombatController.SkillBeingTargeted.Range;
                DrawTrajectory(skillTrajectoryStartPoint, skillTrajectoryEndPoint, ValidDestination, characterCombatController.SkillBeingTargeted.TrajectoryArc);
            }
            else if (DrawCharacterPath && GlobalTBModeController.Instance.IsTurnBased && !character.IsMoving)
            {
                NavMeshHit navHit;
                var distanceToNavMesh = 0.2f;
                var targetPoint = hit.point;

                if (hit.collider.gameObject.CompareTag("Interactable"))
                {
                    distanceToNavMesh = hit.collider.gameObject.GetComponent<Interactable>().InteractionDistance;
                    var lowestPointInCollider = hit.collider.gameObject.GetComponent<Collider>().bounds.min.y;
                    targetPoint = new Vector3(targetPoint.x, lowestPointInCollider, targetPoint.z);
                }
                if (!NavMesh.SamplePosition(targetPoint, out navHit, distanceToNavMesh, NavMesh.AllAreas))
                {
                    return;
                }

                StartCoroutine(character.GetPathEnumerator(hit.point, res =>
                {
                    var path = res;
                    if (path.Length > 1)
                    {
                        PathLine.enabled = true;
                        TargetCircle.enabled = true;
                        var pathLength = PathDistance.CalculatePathLength(path);

                        ValidDestination = pathLength <= characterCombatController.MovementLeft;

                        DrawLine(PathLine, path, ValidDestination);
                        DrawTargetCircle(targetPoint, ValidDestination);
                    }
                }));
            }
        }
    }

    private void DrawLine(LineRenderer lineRenderer, Vector3[] pathCorners, bool validDestination, float finalPointOffset = 0.8f)
    {
        lineRenderer.colorGradient = validDestination ? gradientValid : gradientInvalid;
        lineRenderer.positionCount = pathCorners.Length;
        var lastPointOffset = Vector3.MoveTowards(pathCorners[pathCorners.Length - 1], pathCorners[pathCorners.Length - 2], finalPointOffset);
        pathCorners[pathCorners.Length - 1] = lastPointOffset;
        lineRenderer.SetPositions(pathCorners);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="startPoint"></param>
    /// <param name="endPoint"></param>
    /// <param name="validDestination"></param>
    /// <param name="arcHeight">Calculates height of the arc by applying this as multiplier to distance between start point and end point divided by two.</param>
    public void DrawTrajectory(Vector3 startPoint, Vector3 endPoint, bool validDestination, float arcHeight = 0.5f)
    {
        var middlePoint = Vector3.Lerp(startPoint, endPoint, 0.5f);
        middlePoint.y += arcHeight * Vector3.Distance(startPoint, endPoint) / 2;

        const int cornerCount = 50;
        var pathCorners = new Vector3[cornerCount];

        for (var i = 0; i < cornerCount; i++)
        {
            var t = (float) i / cornerCount;
            pathCorners[i] = Vector3.Lerp(Vector3.Lerp(startPoint, middlePoint, t), Vector3.Lerp(middlePoint, endPoint, t), t);
        }

        DrawLine(TrajectoryRenderer, pathCorners, validDestination, 0);
    }

    private void DrawTargetCircle(Vector3 targetPoint, bool validDestination)
    {
        TargetCircle.colorGradient = validDestination ? gradientValid : gradientInvalid;

        TargetCircleTransform.position = targetPoint;

        const float radius = 0.5f;
        const int segments = 32;
        var drawPoints = new Vector3[segments+1];
        TargetCircle.positionCount = drawPoints.Length;

        for (var i = 0; i < segments+1; i++) {
            var rad = Mathf.Deg2Rad * (i * 360f / segments);
            var x = Mathf.Sin(rad) * radius;
            var y = 0.1f;
            var z = Mathf.Cos(rad) * radius;
            drawPoints[i] = new Vector3(x, y, z);
        }

        TargetCircle.SetPositions(drawPoints);
    }

    public void ChangeCharacter(PlayerMovement cha)
    {
        character = cha;
        agent = cha.GetComponent<NavMeshAgent>();
        characterCombatController = cha.GetComponent<CharacterCombatController>();
    }
}