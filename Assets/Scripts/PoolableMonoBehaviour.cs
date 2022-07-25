
using UnityEngine;

public abstract class PoolableMonoBehaviour : MonoBehaviour
{
	public abstract void WakeUp();
	public abstract void Sleep();
}
