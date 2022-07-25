using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent;
using System.Linq;

public class ObjectPoolManager : MonoBehaviour
{
	private class GameObjectPool
	{
		public GameObject prefab;
		public ConcurrentBag<GameObject> pooledGOs;
	}

	private const string PREFAB_DIR = "Prefabs/PoolableGameObjects";

	public static ObjectPoolManager Instance;
	private Dictionary<System.Type, GameObjectPool> _pools;

	private void Awake()
	{
		if (Instance != null)
		{
			Debug.LogWarning("GameObjectPoolManager already exists");
			Destroy(gameObject);
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
		_pools = new();
		CreatePools();
	}

	private void CreatePools()
	{
		for (int i = 0; i < _pools.Count; ++i)
			for (int j = 0; j < _pools.ElementAt(i).Value.pooledGOs.Count; ++j)
				Destroy(_pools.ElementAt(i).Value.pooledGOs.ElementAt(j));
		_pools = new();
		Object[] objs = Resources.LoadAll(PREFAB_DIR);
		foreach (Object obj in objs)
		{
			if (obj is not GameObject)
			{
				Debug.LogError($"Object {obj.name} is not a GameObject");
				continue;
			}
#if UNITY_EDITOR
			if (UnityEditor.PrefabUtility.GetPrefabAssetType(obj) == UnityEditor.PrefabAssetType.NotAPrefab)
			{
				Debug.LogError($"Object {obj.name} is not a prefab");
				continue;
			}
#endif
			GameObject go = (GameObject)obj;
			if (go.GetComponent<PoolableMonoBehaviour>() == null)
			{
				Debug.LogError($"Object {obj.name} does not contain PoolableMonoBehaviour component");
				continue;
			}
			_pools.Add(go.GetComponent<PoolableMonoBehaviour>().GetType(), new GameObjectPool() { prefab = go, pooledGOs = new() });
		}
	}

	public GameObject Get<T>() where T : PoolableMonoBehaviour
	{
		GameObject go = null;
		if (!_pools[typeof(T)].pooledGOs.TryTake(out go))
			go = Instantiate(_pools[typeof(T)].prefab);
		go.GetComponent<PoolableMonoBehaviour>().WakeUp();
		return go;
	}

	public void Return<T>(GameObject go) where T : PoolableMonoBehaviour
	{
		go.GetComponent<T>().Sleep();
		_pools[typeof(T)].pooledGOs.Add(go);
	}
}
