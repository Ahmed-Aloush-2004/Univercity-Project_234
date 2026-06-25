



using UnityEngine;

public class BucketFluidManager : MonoBehaviour
{
    [Header("References")]
    public ComputeShader bucketFluidShader;
    public BucketKinematics bucketKinematics;
    public Transform paintBucket;

    [Header("Simulation Settings")]
    public int particleAmount = 3000;
    public float cylinderRadius = 0.46f;
    public float cylinderHeight = 1.8f;

    [Header("Hole Settings")]
    [Tooltip("??? ??? ?????? ????? 0.1 ?? 0.15")]
    public float holeRadius = 0.15f;

    [Header("SPH Physics")]
    public float smoothingRadius = 0.15f;
    public float targetDensity = 25f;
    public float pressureMultiplier = 800f;

    [Header("Rheology (Non-Newtonian Paint)")]
    public float baseViscosity = 0.5f;
    public float minViscosity = 0.01f;
    public float shearThinningCoeff = 5.0f;

    [Header("Rendering")]
    public Mesh particleMesh;
    public Material particleMaterial;
    public float particleRenderSize = 0.08f;

    struct Particle
    {
        public Vector3 position; public Vector3 velocity; public float density; public float pressure;
    }

    private ComputeBuffer particleBuffer;
    private Particle[] particleArray;
    private Matrix4x4[] matrices;
    private int kernel;

    void Start()
    {
        particleArray = new Particle[particleAmount];
        matrices = new Matrix4x4[particleAmount];

        // ??????? ??????
        int side = Mathf.CeilToInt(Mathf.Pow(particleAmount, 1f / 3f));
        float spacing = (cylinderRadius * 1.5f) / side;
        int i = 0;

        for (int y = 0; y < side * 2 && i < particleAmount; y++)
        {
            for (int x = 0; x < side && i < particleAmount; x++)
            {
                for (int z = 0; z < side && i < particleAmount; z++)
                {
                    float posX = (x - side / 2f) * spacing;
                    float posZ = (z - side / 2f) * spacing;
                    float posY = -cylinderHeight / 2f + 0.1f + (y * spacing);

                    if (new Vector2(posX, posZ).magnitude <= cylinderRadius - 0.1f)
                    {
                        particleArray[i].position = new Vector3(posX, posY, posZ);
                        particleArray[i].velocity = Vector3.zero;
                        i++;
                    }
                }
            }
        }

        float fallbackY = -cylinderHeight / 2f + 0.1f;
        while (i < particleAmount)
        {
            particleArray[i].position = new Vector3(0, fallbackY, 0);
            particleArray[i].velocity = Vector3.zero;
            fallbackY += 0.015f;
            i++;
        }

        particleBuffer = new ComputeBuffer(particleAmount, 32);
        particleBuffer.SetData(particleArray);
        kernel = bucketFluidShader.FindKernel("CSMain");
        bucketFluidShader.SetBuffer(kernel, "particles", particleBuffer);
    }

    void Update()
    {
        if (bucketKinematics == null || paintBucket == null) return;

        Vector3 worldGEff = bucketKinematics.effectiveGravity;
        Vector3 localGEff = paintBucket.InverseTransformDirection(worldGEff);

        float safeDeltaTime = Mathf.Min(Time.deltaTime, 0.016f);
        bucketFluidShader.SetFloat("deltaTime", safeDeltaTime);

        bucketFluidShader.SetVector("localEffectiveGravity", localGEff);
        bucketFluidShader.SetInt("totalParticles", particleAmount);
        bucketFluidShader.SetFloat("cylinderRadius", cylinderRadius);
        bucketFluidShader.SetFloat("cylinderHeight", cylinderHeight);
        bucketFluidShader.SetFloat("holeRadius", holeRadius);

        bucketFluidShader.SetFloat("smoothingRadius", smoothingRadius);
        bucketFluidShader.SetFloat("targetDensity", targetDensity);
        bucketFluidShader.SetFloat("pressureMultiplier", pressureMultiplier);
        bucketFluidShader.SetFloat("baseViscosity", baseViscosity);
        bucketFluidShader.SetFloat("minViscosity", minViscosity);
        bucketFluidShader.SetFloat("shearThinningCoeff", shearThinningCoeff);

        int threadGroups = Mathf.CeilToInt(particleAmount / 64f);
        bucketFluidShader.Dispatch(kernel, threadGroups, 1, 1);

        particleBuffer.GetData(particleArray);
        Matrix4x4 bucketMatrix = paintBucket.localToWorldMatrix;

        for (int i = 0; i < particleAmount; i++)
        {
            // ?????? ???????: ??? ??? ?????? ??? (?? ???????)? ???? ???? ???? ??? 0
            if (particleArray[i].position.y < -1000f)
            {
                matrices[i] = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.zero);
            }
            else
            {
                // ??? ??? ????? ????? ???? ?????
                Vector3 worldPos = bucketMatrix.MultiplyPoint3x4(particleArray[i].position);
                matrices[i] = Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * particleRenderSize);
            }
        }
        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, matrices, particleAmount);
    }

    void OnDestroy()
    {
        if (particleBuffer != null) particleBuffer.Release();
    }
}
