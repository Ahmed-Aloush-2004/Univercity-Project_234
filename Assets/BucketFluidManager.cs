using UnityEngine;

public class BucketFluidManager : MonoBehaviour
{
    [Header("References")]
    public ComputeShader bucketFluidShader;
    public BucketKinematics bucketKinematics; // ??? ?? ?????? ?????????
    public Transform paintBucket; // ????????? ?????

    [Header("Simulation Settings")]
    public int particleAmount = 1500; // ??? ????? ??????? ???????
    public float cylinderRadius = 0.5f;
    public float cylinderHeight = 2.0f;

    [Header("Rendering")]
    public Mesh particleMesh;
    public Material particleMaterial;
    public float particleRenderSize = 0.06f;

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

        // ????? ???????? ?? ????? ?????? ?? ????????? (???? ?????)
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

    void Update()
    {
        if (bucketKinematics == null || paintBucket == null) return;

        // 1. ??????? ???????? ??????? ???????? ?????? ????? ???????
        // ??? ?? ??????? ?????? ?? g_eff ?? ?????? ????????!
        Vector3 worldGEff = bucketKinematics.effectiveGravity;
        Vector3 localGEff = paintBucket.InverseTransformDirection(worldGEff);

        // 2. ????? ???????? ???? ??????
        bucketFluidShader.SetFloat("deltaTime", Time.deltaTime);
        bucketFluidShader.SetVector("localEffectiveGravity", localGEff);
        bucketFluidShader.SetInt("totalParticles", particleAmount);
        bucketFluidShader.SetFloat("cylinderRadius", cylinderRadius);
        bucketFluidShader.SetFloat("cylinderHeight", cylinderHeight);

        int threadGroups = Mathf.CeilToInt(particleAmount / 64f);
        bucketFluidShader.Dispatch(kernel, threadGroups, 1, 1);

        // 3. ?????: ????? ?????? ?????? ?????? ??? ???? ????? ???? ?????
        particleBuffer.GetData(particleArray);
        Matrix4x4 bucketMatrix = paintBucket.localToWorldMatrix;

        for (int i = 0; i < particleAmount; i++)
        {
            // ????? ?????? ??????? ??? ???? ?????? ??????
            Vector3 worldPos = bucketMatrix.MultiplyPoint3x4(particleArray[i].position);
            matrices[i] = Matrix4x4.TRS(worldPos, Quaternion.identity, Vector3.one * particleRenderSize);
        }

        Graphics.DrawMeshInstanced(particleMesh, 0, particleMaterial, matrices, particleAmount);
    }

    void OnDestroy()
    {
        if (particleBuffer != null) particleBuffer.Release();
    }
}