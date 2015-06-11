//
// Scroller script for Wall
//
using UnityEngine;

namespace Kvant
{
    [RequireComponent(typeof(Wall)), AddComponentMenu("Kvant/Wall Scroller")]
    public class WallScroller : MonoBehaviour
    {
        public Vector2 speed;

        Vector3 _origin;
        Vector2 _position;

        void Start()
        {
            _origin = transform.position;
        }

        void Update()
        {
            var wall = GetComponent<Wall>();
            var wall_tr = wall.transform;

            _position += speed * Time.deltaTime;

            var ex = wall.extent;
            var dx = ex.x / wall.columns;
            var dy = ex.y / wall.rows;

            var ox = _position.x % dx;
            var oy = _position.y % dy;

            transform.position = _origin - wall_tr.right * ox - wall_tr.up * oy;
            wall.offset = new Vector2(_position.x - ox, _position.y - oy);
        }
    }
}
