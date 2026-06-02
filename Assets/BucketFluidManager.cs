//using UnityEngine;

//public class BucketFluidManager : MonoBehaviour
//{
//    [Header("References")]
//    public ComputeShader bucketFluidShader;
//    public BucketKinematics bucketKinematics;
//    public Transform paintBucket;

//    [Header("Simulation Settings")]
//    public int particleAmount = 10000;
//    public float cylinderRadius = 0.5f;
//    public float cylinderHeight = 2.0f;

//    [Header("Physics Settings")]
//    [Tooltip("??? ??? ???????: ???? ????? ????????? ??? ????")]
//    public float smoothingRadius = 0.45f;
//    [Tooltip("??????? ?????????: ??? ???? ?????? ?? ??????")]
//    public float targetDensity = 10.0f;
//    [Tooltip("????? ?????: ??? ??? ?????? ?? ????????")]
//    public float pressureMultiplier = 1500.0f;
//    [Tooltip("???????: ??? ????? ??????")]
//    public float viscosity = 0.1f;

//    [Header("Rendering")]
//    public Mesh particleMesh;
//    public Material particleMaterial;
//    public float particleRenderSize = 0.15f;

//    struct Particle
//    {
//        public Vector3 position;
//        public Vector3 velocity;
//        public float density;
//        public float pressure;
//    }

//    private ComputeBuffer particleBuffer;
//    private Particle[] particleArray;
//    private Matrix4x4[] matrices;
//    private int kernel;

//    void Start()
//    {
//        particleArray = new Particle[particleAmount];
//        matrices = new Matrix4x4[particleAmount];

//        for (int i = 0; i < particleAmount; i++)
//        {
//            Vector2 randomCircle = Random.insideUnitCircle * (cylinderRadius - 0.1f);
//            float randomY = Random.Range(-cylinderHeight / 2f + 0.1f, 0f);

//            particleArray[i].position = new Vector3(randomCircle.x, randomY, randomCircle.y);
//            particleArray[i].velocity = Vector3.zero;
//        }

//        particleBuffer = new ComputeBuffer(particleAmount, 32);
//        particleBuffer.SetData(particleArray);
//        kernel = bucketFluidShader.FindKernel("CSMain");
//        bucketFluidShader.SetBuffer(kernel, "particles", particleBuffer);
//    }

//    void FixedUpdate()
//    {
//        if (bucketKinematics == null || paintBucket == null || particleBuffer == null) return;

//        Vector3 worldGEff = bucketKinematics.effectiveGravity;
//        Vector3 localGEff = paintBucket.InverseTransformDirection(worldGEff);

//        // ????? ????????? ??????
//        bucketFluidShader.SetFloat("deltaTime", Time.fixedDeltaTime);
//        bucketFluidShader.SetVector("localEffectiveGravity", localGEff);
//        bucketFluidShader.SetInt("totalParticles", particleAmount);
//        bucketFluidShader.SetFloat("cylinderRadius", cylinderRadius);
//        bucketFluidShader.SetFloat("cylinderHeight", cylinderHeight);

//        // ????? ????????? ?????????? ??????? ?? ??? Inspector
//        bucketFluidShader.SetFloat("smoothingRadius", smoothingRadius);
//        bucketFluidShader.SetFloat("targetDensity", targetDensity);
//        bucketFluidShader.SetFloat("pressureMultiplier", pressureMultiplier);
//        bucketFluidShader.SetFloat("viscosity", viscosity);

//        int threadGroups = Mathf.CeilToInt(particleAmount / 64f);
//        bucketFluidShader.Dispatch(kernel, threadGroups, 1, 1);
//    }

//    void Update()
//    {
//        if (particleBuffer == null || paintBucket == null) return;

//        particleBuffer.GetData(particleArray);
//        Matrix4x4 bucketMatrix = paintBucket.localToWorldMatrix;

//        for (int i = 0; i < particleAmount; i++)
//        {
//            Vector3 worldPos = bucketMatrix.MultiplyPoint3x4(particleArray[i].position);
//            matrices[i] = Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * particleRenderSize);
//        }

//        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, matrices, particleAmount);
//    }

//    void OnDestroy()
//    {
//        if (particleBuffer != null)
//        {
//            particleBuffer.Release();
//            particleBuffer = null;
//        }
//    }
//}











using UnityEngine;

public class BucketFluidManager : MonoBehaviour
{
    [Header("References")]
    public ComputeShader bucketFluidShader;
    public BucketKinematics bucketKinematics;
    public Transform paintBucket;

    [Header("Simulation Settings")]
    public int particleAmount = 6000;
    public float cylinderRadius = 0.5f;
    public float cylinderHeight = 2.0f;

    [Header("Physics Settings")]
    [Tooltip("??? ??? ???????: ???? ????? ????????? ??? ????")]
    public float smoothingRadius = 0.15f;
    [Tooltip("??????? ?????????: ??? ???? ?????? ?? ??????")]
    public float targetDensity = 45.0f;
    [Tooltip("????? ?????: ??? ??? ?????? ?? ????????")]
    public float pressureMultiplier = 350.0f;
    [Tooltip("???????: ??? ????? ??????")]
    public float viscosity = 0.05f;

    [Header("Rendering")]
    public Mesh particleMesh;
    public Material particleMaterial;
    public float particleRenderSize = 0.15f;

    struct Particle
    {
        public Vector3 position;
        public Vector3 velocity;
        public float density;
        public float pressure;
    }

    private ComputeBuffer particleBuffer;
    private Particle[] particleArray;
    private Matrix4x4[] matrices;
    private int kernel;

    void Start()
    {
        particleArray = new Particle[particleAmount];
        matrices = new Matrix4x4[particleAmount];

        // ????? ???????? ???? ?????? ???? ??????
        for (int i = 0; i < particleAmount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * (cylinderRadius - 0.1f);
            float randomY = Random.Range(-cylinderHeight / 2f + 0.1f, 0f);

            particleArray[i].position = new Vector3(randomCircle.x, randomY, randomCircle.y);
            particleArray[i].velocity = Vector3.zero;
        }

        particleBuffer = new ComputeBuffer(particleAmount, 32);
        particleBuffer.SetData(particleArray);
        kernel = bucketFluidShader.FindKernel("CSMain");
        bucketFluidShader.SetBuffer(kernel, "particles", particleBuffer);
    }

    void FixedUpdate()
    {
        if (bucketKinematics == null || paintBucket == null || particleBuffer == null) return;

        Vector3 worldGEff = bucketKinematics.effectiveGravity;
        Vector3 localGEff = paintBucket.InverseTransformDirection(worldGEff);

        // ????? ????????? ??????
        bucketFluidShader.SetFloat("deltaTime", Time.fixedDeltaTime);
        bucketFluidShader.SetVector("localEffectiveGravity", localGEff);
        bucketFluidShader.SetInt("totalParticles", particleAmount);
        bucketFluidShader.SetFloat("cylinderRadius", cylinderRadius);
        bucketFluidShader.SetFloat("cylinderHeight", cylinderHeight);

        // ????? ????????? ?????????? ?? ??? Inspector
        bucketFluidShader.SetFloat("smoothingRadius", smoothingRadius);
        bucketFluidShader.SetFloat("targetDensity", targetDensity);
        bucketFluidShader.SetFloat("pressureMultiplier", pressureMultiplier);
        bucketFluidShader.SetFloat("viscosity", viscosity);

        int threadGroups = Mathf.CeilToInt(particleAmount / 64f);
        bucketFluidShader.Dispatch(kernel, threadGroups, 1, 1);
    }

    void Update()
    {
        if (particleBuffer == null || paintBucket == null) return;

        particleBuffer.GetData(particleArray);
        Matrix4x4 bucketMatrix = paintBucket.localToWorldMatrix;

        for (int i = 0; i < particleAmount; i++)
        {
            Vector3 worldPos = bucketMatrix.MultiplyPoint3x4(particleArray[i].position);
            matrices[i] = Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * particleRenderSize);
        }

        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, matrices, particleAmount);
    }

    void OnDestroy()
    {
        if (particleBuffer != null)
        {
            particleBuffer.Release();
            particleBuffer = null;
        }
    }
}