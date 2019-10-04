using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BulletCreator
{

    public class BulletPattern : MonoBehaviour
    {
        public AnimationCurve vRotation = new AnimationCurve();
        public AnimationCurve velocity = new AnimationCurve();
        public AnimationCurve rotation = new AnimationCurve();
    }

    public struct CreateBullets
    {
        public BulletGroup Grouping(GameObject[] bullets, Transform[] transforms)
        {
            if(bullets.Length > transforms.Length)
            {
                Debug.LogError("Invalid information for grouping");
                return null;
            }
            BulletGroup bulletGroup = new BulletGroup();
            int i = 0;
            foreach(GameObject bullet in bullets)
            {
                bullet.transform.SetParent(bulletGroup.parent.transform);
                bullet.transform.localPosition = transforms[i].position;
                bullet.transform.localRotation = transforms[i].rotation;
                bulletGroup.bullets.Add(bullet);
                i++;
            }
            return bulletGroup;
        }
    }
}
