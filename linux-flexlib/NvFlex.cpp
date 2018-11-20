//#include "NvFlex.h"

//#define LOGI(...) ((void)__android_log_print(ANDROID_LOG_INFO, "NvFlex", __VA_ARGS__))
//#define LOGW(...) ((void)__android_log_print(ANDROID_LOG_WARN, "NvFlex", __VA_ARGS__))
//
//extern "C" {
//	/* This trivial function returns the platform ABI for which this dynamic native library is compiled.*/
//	const char * NvFlex::getPlatformABI()
//	{
//	#if defined(__arm__)
//	#if defined(__ARM_ARCH_7A__)
//	#if defined(__ARM_NEON__)
//		#define ABI "armeabi-v7a/NEON"
//	#else
//		#define ABI "armeabi-v7a"
//	#endif
//	#else
//		#define ABI "armeabi"
//	#endif
//	#elif defined(__i386__)
//		#define ABI "x86"
//	#else
//		#define ABI "unknown"
//	#endif
//		LOGI("This dynamic shared library is compiled with ABI: %s", ABI);
//		return "This native library is compiled with ABI: %s" ABI ".";
//	}
//
//	void NvFlex()
//	{
//	}
//
//	NvFlex::NvFlex()
//	{
//	}
//
//	NvFlex::~NvFlex()
//	{
//	}
//}

#include <string.h>
#include <float.h>
#include <math.h>

#ifdef _WIN32
#   define DLLEXPORT __declspec(dllexport)
#else
#   define DLLEXPORT
#endif

class ID3D11Resource;
class ID3D11Device;

// Not used
extern "C" DLLEXPORT ID3D11Device* flexUtilsDeviceFromResource(ID3D11Resource* resource)
{
    return 0;
}

// Fast copy from/to managed memory
extern "C" DLLEXPORT void flexUtilsFastCopy(const char* src, int srcOfs, char* dst, int dstOfs, int size)
{
    memcpy(dst + dstOfs, /*size,*/ src + srcOfs, size);
}

static inline float min(const float a, const float b) { return a < b ? a : b; }
static inline float max(const float a, const float b) { return a > b ? a : b; }

struct float4
{
    float x, y, z, w;

    inline float4() {}
    inline float4(float x, float y, float z, float w) : x(x), y(y), z(z), w(w) {}

    inline float4 operator - (const float4& v) const { return float4(x - v.x, y - v.y, z - v.z, w - v.w); }

    inline static float dot(const float4& a, const float4& b) { return a.x * b.x + a.y * b.y + a.z * b.z + a.w * b.w; }
    inline static float length2(const float4& v) { return dot(v, v); }
};

struct float3
{
    float x, y, z;

    inline float3() {}
    inline float3(float x, float y, float z) : x(x), y(y), z(z) {}
    inline float3(const float4& v) : x(v.x), y(v.y), z(v.z) {}
    explicit inline float3(float s) : x(s), y(s), z(s) {}

    inline float3 operator - () const { return float3(-x, -y, -z); }
    inline float3 operator - (const float3& v) const { return float3(x - v.x, y - v.y, z - v.z); }
    inline float3 operator * (const float3& v) const { return float3(x * v.x, y * v.y, z * v.z); }
    inline float3 operator * (float s) const { return float3(x * s, y * s, z * s); }
    inline float3 operator / (float s) const { return float3(x / s, y / s, z / s); }
    inline float3& operator /= (float s) { x /= s; y /= s; z /= s; return *this; }

    inline static float dot(const float3& a, const float3& b) { return a.x * b.x + a.y * b.y + a.z * b.z; }
    inline static float length2(const float3& v) { return dot(v, v); }
    inline static float length(const float3& v) { return sqrt(length2(v)); }
    inline static float3 cross(const float3& a, const float3& b) { return float3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x); }
    inline static float3 min(const float3& a, const float3& b) { return float3(::min(a.x, b.x), ::min(a.y, b.y), ::min(a.z, b.z)); }
    inline static float3 max(const float3& a, const float3& b) { return float3(::max(a.x, b.x), ::max(a.y, b.y), ::max(a.z, b.z)); }
};

extern "C" DLLEXPORT int flexUtilsPickParticle(const float3& origin, const float3& dir, const float4* particles, const int* phases, int n, float radius)
{
    float maxDistSq = radius * radius;
    float minT = FLT_MAX;
    int minIndex = -1;

    for (int i = 0; i < n; ++i)
    {
        if (phases && phases[i] & (1 << 26))
            continue;

        float3 delta = float3(particles[i]) - origin;
        float t = float3::dot(delta, dir);

        if (t > 0.0f)
        {
            float3 perp = delta - dir * t;

            float dSq = float3::length2(perp);

            if (dSq < maxDistSq && t < minT)
            {
                minT = t;
                minIndex = i;
            }
        }
    }

    return minIndex;
}

extern "C" DLLEXPORT int flexUtilsConvexPlanes(const float3* meshVertices, const float3& localScale, const int* meshTriangles, int triangleCount, float4* planes, float3* bounds)
{
    int planeCount = 0;
    float3 boundsMin = float3(FLT_MAX, FLT_MAX, FLT_MAX);
    float3 boundsMax = float3(-FLT_MAX, -FLT_MAX, -FLT_MAX);
    for (int i = 0; i < triangleCount; ++i)
    {
        float3 p0 = meshVertices[meshTriangles[i * 3 + 0]] * localScale;
        float3 p1 = meshVertices[meshTriangles[i * 3 + 1]] * localScale;
        float3 p2 = meshVertices[meshTriangles[i * 3 + 2]] * localScale;
        boundsMin = float3::min(boundsMin, p0);
        boundsMax = float3::max(boundsMax, p0);
        boundsMin = float3::min(boundsMin, p1);
        boundsMax = float3::max(boundsMax, p1);
        boundsMin = float3::min(boundsMin, p2);
        boundsMax = float3::max(boundsMax, p2);
        float3 n = float3::cross(p1 - p0, p2 - p0);
        float l2 = float3::length2(n);
        if (l2 > FLT_EPSILON)
        {
            n /= sqrt(l2);
            float d = float3::dot(n, p0);
            float4 p(n.x, n.y, n.z, -d);
            bool unique = true;
            for (int i = 0; i < planeCount; ++i)
            {
                if (float4::length2(p - planes[i]) < FLT_EPSILON)
                {
                    unique = false;
                    break;
                }
            }
            if (unique)
                planes[planeCount++] = p;
        }
    }
    bounds[0] = boundsMin;
    bounds[1] = boundsMax;
    return planeCount;
}

extern "C" DLLEXPORT void flexUtilsClothRefPoints(const float4* particles, int count, int* refPoints)
{
    refPoints[0] = 0;
    refPoints[1] = 1;
    refPoints[2] = 2;
    float maxArea2 = 0;
    for (int i = 0; i < count; ++i)
    {
        for (int j = i + 1; j < count; ++j)
        {
            for (int k = j + 1; k < count; ++k)
            {
                float3 pi = particles[i], pj = particles[j], pk = particles[k];
                float area2 = float3::length2(float3::cross(pj - pi, pk - pi));
                if (area2 > maxArea2)
                {
                    maxArea2 = area2;
                    refPoints[0] = i;
                    refPoints[1] = j;
                    refPoints[2] = k;
                }
            }
        }
    }
}

extern "C" DLLEXPORT void flexUtilsComputeBounds(const float4* particles, const int* indices, int count, float3* boundsMin, float3* boundsMax)
{
    float3 aabbMin(FLT_MAX), aabbMax(-FLT_MAX);
    for (int i = 0; i < count; ++i)
    {
        float3 p = particles[indices[i]];
        aabbMin = float3::min(aabbMin, p);
        aabbMax = float3::max(aabbMax, p);
    }
    *boundsMin = aabbMin;
    *boundsMax = aabbMax;
}
