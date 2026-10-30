namespace AE
{
    using System.Collections.Generic;
    using UnityEngine;

    public class PriorityQueueDemo : MonoBehaviour
    {
        [SerializeField] private Cube[] _cubes;
        private PriorityQueue<Cube> _queue;
        private float _timer = 0;
        private float _dequeueInterval = 2.0f;

        void Start()
        {
            _queue = new PriorityQueue<Cube>();

            // Enqueue objects with their priorities
            foreach (Cube obj in _cubes)
            {
                int priority = Random.Range(1, 100); // Assign a random priority
                _queue.Enqueue(obj, priority);
            }
        }

        void Update()
        {
            _timer += Time.deltaTime;

            if (_timer >= _dequeueInterval)
            {
                if (!_queue.IsEmpty())
                {
                    Cube cube = _queue.Dequeue();
                    ProcessCube(cube);
                }
                _timer = 0;
            }
        }

        private void ProcessCube(Cube cube)
        {
            var renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.magenta;
            }
        }
    }
}