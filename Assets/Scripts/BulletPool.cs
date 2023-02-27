using System;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
	public List<GameObject> bullets;
	public List<GameObject> bulletsPool;
	public List<GameObject> activated;
	public int bulletCount;
	public GameObject parent = null;

	void Awake()
	{

		for (int i = 0; i < bullets.Count; i++)
		{
			for (int x = 0; x < bulletCount; x++)
			{
				if (parent != null)
				{
					GameObject enemyCopy = Instantiate(bullets[i], parent.transform);
					bulletsPool.Add(enemyCopy);
					enemyCopy.SetActive(false);
				}
				else
				{
					GameObject enemyCopy = Instantiate(bullets[i]);
					bulletsPool.Add(enemyCopy);
					enemyCopy.SetActive(false);
				}

			}
		}
	}

	public void Spawn(Vector3 position, Action<GameObject> callback = null)
	{

		if (bulletsPool.Count != 0)
		{
			int rndEnemy = UnityEngine.Random.Range(0, bulletsPool.Count);
			bulletsPool[rndEnemy].transform.position = position;
			bulletsPool[rndEnemy].SetActive(true);
			activated.Add(bulletsPool[rndEnemy]);
			bulletsPool.Remove(bulletsPool[rndEnemy]);
			callback?.Invoke(activated[activated.Count - 1]);
		}
	}

	public void Spawn(Vector3 position)
	{

		if (bulletsPool.Count != 0)
		{
			int rndEnemy = UnityEngine.Random.Range(0, bulletsPool.Count);
			bulletsPool[rndEnemy].transform.position = position;
			bulletsPool[rndEnemy].SetActive(true);
			activated.Add(bulletsPool[rndEnemy]);
			bulletsPool.Remove(bulletsPool[rndEnemy]);
		}
	}

	public void Spawn(Vector3 position, GameObject bulletName = null, Action<GameObject> callback = null)
	{

		if (bulletName != null && bulletsPool.Count != 0)
		{
			GameObject getBullet = bulletsPool.Find((x) => x.name.Contains(bulletName.name));
			getBullet.transform.position = position;
			getBullet.SetActive(true);
			activated.Add(getBullet);
			bulletsPool.Remove(getBullet);
			callback?.Invoke(activated[activated.Count - 1]);

		}
	}

	public void Despawn(GameObject bullet)
	{

		bullet.SetActive(false);
		bulletsPool.Add(bullet);
		activated.Remove(bullet);
	}
}
