using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace _Scripts
{
    public class MeshGenerator : MonoBehaviour
    {
        [SerializeField] private MeshSettings _settings;

        private MeshFilter _meshFilter;
        private Mesh _mesh;
        private MeshRenderer _meshRenderer;

        private Vector3[] _vertices;
        private Vector3[] _points;
        private readonly Vector4[] _rippleOrigins = new Vector4[300];

        private static readonly int GameTime = Shader.PropertyToID("_GameTime");
        private static readonly int Param4 = Shader.PropertyToID("_Param4");
        private static readonly int Param3 = Shader.PropertyToID("_Param3");
        private static readonly int Param2 = Shader.PropertyToID("_Param2");
        private static readonly int Param1 = Shader.PropertyToID("_Param1");
        private static readonly int Ripples = Shader.PropertyToID("_Ripples");
        private static readonly int RippleNum = Shader.PropertyToID("_RippleNum");
        private static readonly int Speed = Shader.PropertyToID("_Speed");
        private static readonly int FadeTime = Shader.PropertyToID("_FadeTime");
        
        private int _rippleCount;
        private int _rippleCountForShader;
        private readonly List<ParticleCollisionEvent> _collisionEvents = new();
        private int Resolution => _settings.Resolution;
        private float Scale => _settings.Scale;
        Material Material => _settings.Material;


        private void Start()
        {
            if (_settings == null) throw new Exception("Settings are not initialized. Assign this field");
            ConstructMesh();
        }

        #region ConstructMesh

        [ContextMenu("Create")]
        private void ConstructMesh()
        {
            gameObject.AddComponent<MeshFilter>().mesh = _mesh = new Mesh() {name = "Ripple"};
            gameObject.AddComponent<MeshRenderer>().material = Material;


            Material.SetFloat(Param1, _settings.param1);
            Material.SetFloat(Param2, _settings.param2);
            Material.SetFloat(Param3, _settings.param3);
            Material.SetFloat(FadeTime, _settings.FadeTime);
            Material.SetFloat(Speed, _settings.Speed);
            Material.SetVectorArray(Ripples, _rippleOrigins);
            Material.SetFloat(RippleNum, _rippleCount);
            Material.SetFloat(GameTime, Time.time);

            _vertices = new Vector3[(Resolution + 1) * (Resolution + 1)];

            Vector2[] uv = new Vector2[_vertices.Length];
            for (int i = 0, z = 0; z <= Resolution; z++)
            {
                for (int x = 0; x <= Resolution; x++, i++)
                {
                    _vertices[i] = new Vector3(x - Resolution * 0.5f, 0, z - Resolution * 0.5f) * Scale;
                    uv[i] = new Vector2((float) x / Resolution, (float) z / Resolution);
                }
            }

            _mesh.vertices = _vertices;

            int[] triangles = new int[Resolution * Resolution * 6];
            for (int ti = 0, vi = 0, y = 0; y < Resolution; y++, vi++)
            {
                for (int x = 0; x < Resolution; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + Resolution + 1;
                    triangles[ti + 5] = vi + Resolution + 2;
                }
            }

            _mesh.uv = uv;
            _mesh.triangles = triangles;
            _mesh.RecalculateNormals();
        }

        #endregion


        private void Update()
        {
            Material.SetFloat(Param1, _settings.param1);
            Material.SetFloat(Param2, _settings.param2);
            Material.SetFloat(Param3, _settings.param3);
            Material.SetFloat(Param4, _settings.param4);

            Material.SetFloat(Speed, _settings.Speed);
            Material.SetFloat(FadeTime, _settings.FadeTime);

            Material.SetFloat(GameTime, Time.time);


            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                PutRipple();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            other.GetComponent<ParticleSystem>().GetCollisionEvents(gameObject, _collisionEvents);

            foreach (var collisionEvent in _collisionEvents)
            {
                PutRipple(collisionEvent.intersection*Scale);
            }
        }

        private void PutRipple()
        {
            var origin = new Vector3(Random.Range(-10, 10), UnityEngine.Time.time, Random.Range(-10, 10));
            _rippleOrigins[_rippleCount] = origin;
            _rippleCount++;
            if (_rippleCount > 299) _rippleCount = 0;

            Material.SetFloat(RippleNum, _rippleCount);
            Material.SetVectorArray(Ripples, _rippleOrigins);
        }

        private void PutRipple(Vector3 position)
        {
            var origin = new Vector3(position.x, UnityEngine.Time.time, position.z);
            _rippleOrigins[_rippleCount] = origin;
            _rippleCount++;
            _rippleCountForShader++;
            if (_rippleCountForShader > 299) _rippleCountForShader = 299;
            if (_rippleCount > 299) _rippleCount = 0;

            Material.SetFloat(RippleNum, _rippleCountForShader);
            Material.SetVectorArray(Ripples, _rippleOrigins);
        }
    }
}