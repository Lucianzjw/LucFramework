using System;
using System.Collections;
using System.Collections.Generic;
using LucFramework.Scripts.Tools.BindData;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

[InjectClass]
public class SpaceLayout : MonoBehaviour
{
    public float AngleBTCubes= 5; // 每个物体之间的角度
    public float posZoffsetBtCubes = 0.2f; // 每个物体之间的z轴偏移
    
    public int MaxCloumnCount = 5; // 每列最大数量
    public int MaxRowCount = 6; // 每行最大数量
    public int CurRowCount;
    public float Spacing = 0.2f; // 物体之间的间隔
    public GameObject Prefab; // 子物体的预制体
    public int ChildCount => _children.Count; // 子物体数量
    private int _middleIndex = -1; // 中间位置的索引
    private bool _isOdd = false; // 子物体数量是否为奇数
    private List<Transform> _children = new List<Transform>(); // 子物体列表
    
    [SerializeField] TextMeshPro _pageText;
    private BindModel<int> _curPage = new BindModel<int>();
    private BindModel<int> _pageCount = new BindModel<int>();
    private Dictionary<int, (float[], Vector3[])> _layoutDic = new Dictionary<int, (float[], Vector3[])>();

    private int _startIndex;
    private int _endIndex;
    private void Awake()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            _children.Add(transform.GetChild(i));
        }
        
        InitPage();
        LayoutChildren();
    }

    private void InitPage()
    {
        _curPage.AddListenerWithInit(index =>
        {
            _pageText.text = $"{index + 1}/{_pageCount.Value}"; 
            UpdatePage();
        });
        _pageCount.AddListenerWithInit(index => { _pageText.text = $"{_curPage.Value + 1}/{index}"; });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
           RemoveChild(_children[^1].gameObject);
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            AddChild("1");
            LayoutChildren();
        }
    }

    
    public void LayoutChildren()
    {
        //Debug.Log("排版");
        int childCount = _children.Count;

        // 子物体数量满足一行，按照一行的数量排列
        CurRowCount = Mathf.CeilToInt((float)childCount / MaxRowCount);
        _pageCount.Value = (CurRowCount / MaxRowCount)+1;
        for (int i = 0; i < CurRowCount; i++)
        {
            var startIndex = i * MaxRowCount;
            var endIndex = Mathf.Min((i + 1) * MaxRowCount, childCount);
            var count = endIndex - startIndex;
            var offset = (count - 1) * Spacing / 2;
            if (!_layoutDic.ContainsKey(count))
            {
                _layoutDic.Add(count,LayoutInSectorial(count));
            }
            var (posZ,eulerAangles)= _layoutDic[count];
            var culmIndex = i % MaxCloumnCount;
            for (int j = startIndex; j < endIndex; j++)
            {
                //30个为一组，如果j大于29，第30个的index就是0
                var rowIndex = j - startIndex;
                var child = _children[j];
                var position = new Vector3(rowIndex * Spacing - offset, -culmIndex * Spacing, posZ[rowIndex]);
                child.localPosition = position;
                child.localEulerAngles = eulerAangles[rowIndex];
            }
        }

        UpdatePage();
    }

    /// <summary>
    /// 排版
    /// </summary>
    /// <param name="total"></param>
    /// <returns></returns>
    (float[] posZ, Vector3[] eulerAngles) LayoutInSectorial(int total)
    {
        Debug.Log(">>>>>>>>"+total);
        var posZ = new float[total];
        var eulerAngles = new Vector3[total];
        var isOdd = total % 2 == 1;
        var middle = total / 2;
        var inIndex = middle;
        if (isOdd)
        {
            posZ[middle] = 0;
            eulerAngles[middle] = Vector3.zero;
            
            for (int i = middle-1; i >= 0; i--)
            {
                var deIndex = i;
                inIndex++;
                
                posZ[deIndex]= posZoffsetBtCubes * (middle - deIndex);
                eulerAngles[deIndex] = new Vector3(0, AngleBTCubes * (middle - deIndex), 0);
                
                posZ[inIndex] = posZoffsetBtCubes * (inIndex - middle);
                eulerAngles[inIndex] = new Vector3(0, AngleBTCubes * (inIndex - middle) * (-1), 0);
                
                Debug.Log($"<--第{deIndex}个，total：{total}>>z:{posZ[deIndex]}>>eul:{eulerAngles[deIndex]}");
                Debug.Log($"-->第{inIndex}个，total：{total}>>z:{posZ[inIndex]}>>eul:{eulerAngles[inIndex]}");
            }
        }
        else
        {
            posZ[middle-1] = posZoffsetBtCubes / 2f;
            eulerAngles[middle-1] = new Vector3(0, AngleBTCubes, 0);
            posZ[middle] = posZoffsetBtCubes / 2f;
            eulerAngles[middle] = new Vector3(0, AngleBTCubes * (-1) , 0);
            
            for (int i = middle-2; i >= 0; i--)
            {
                var deIndex = i;
                inIndex++;

                posZ[deIndex] = posZoffsetBtCubes * (middle - deIndex - 0.5f);
                eulerAngles[deIndex] = new Vector3(0, AngleBTCubes * (middle - deIndex)  , 0);

                posZ[inIndex] = posZoffsetBtCubes * (inIndex - middle + 0.5f);
                eulerAngles[inIndex] = new Vector3(0, AngleBTCubes * (inIndex - middle+1)* (-1), 0);
                Debug.Log($"<--第{deIndex}个，total：{total}>>z:{posZ[deIndex]}>>eul:{eulerAngles[deIndex]}>>{AngleBTCubes * (middle - deIndex)}");
                Debug.Log($"-->第{inIndex}个，total：{total}>>z:{posZ[inIndex]}>>eul:{eulerAngles[inIndex]}>>{AngleBTCubes * (inIndex - middle+1)* (-1)}");
            }
        }
        return (posZ, eulerAngles);
    }

    public void CheckCurPageOverBoundary()
    {
        _curPage.Value = _curPage.Value < 0 ? 0 : _curPage.Value;
        _curPage.Value = _curPage.Value >= _pageCount.Value ? _pageCount.Value - 1 : _curPage.Value;
    }

    public void UpdatePage()
    {
        CheckCurPageOverBoundary();
        _startIndex= _curPage.Value * MaxCloumnCount * MaxRowCount;
        _endIndex = Mathf.Min(_children.Count, _startIndex + MaxCloumnCount * MaxRowCount) - 1;
        Debug.Log(_curPage.Value+"    "+_startIndex+"    "+_endIndex);
        for (int i = 0; i < _children.Count; i++)
        {
            _children[i].gameObject.SetActive(i >= _startIndex && i <= _endIndex);
        }
    }

    public void OnNextPage()
    {
        if (_curPage.Value < _pageCount.Value - 1)
            _curPage.Value++;
    }

    public void OnLastPage()
    {
        if(_curPage.Value>0)
            _curPage.Value--;
    }

    public GameObject AddChild(string name)
    {
        var newChild = Instantiate(Prefab, transform).InjectInGameObjectWithChildren();
        _children.Add(newChild.transform);
        if(name.Length>4)
            name = name.Substring(0, 4) + "...";
        //Debug.Log(newChild.name);
        newChild.transform.GetChild(0).GetChild(0).GetComponent<TextMeshPro>().text = name;
        return newChild;
    }
    
    public void RemoveChild(GameObject child)
    {
        _children.Remove(child.transform);
        Destroy(child);
        LayoutChildren();
    }
    
}