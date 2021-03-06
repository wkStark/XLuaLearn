﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using XLua;

public class GetLuaTable : MonoBehaviour
{
    public TextAsset luaScript;     //因为重点学的是获取luatable，所以这里使用最简便的方式获取lua测试脚本，具体加载脚本的方式看002
    LuaEnv luaEnv;
    // Start is called before the first frame update
    void Start()
    {
        luaEnv = new LuaEnv();
        luaEnv.DoString(luaScript.text);

        //访问lua表方式一:映射到普通的class或struct，需要注意的是这种方式是用的值拷贝，如果class比较复杂代价会比较大。而且修改class的字段值不会同步到table
        //反过来也不会，本例中的Class：luaTab
        luaTab _luatable = luaEnv.Global.Get<luaTab>("tab");
        Debug.Log("使用普通class的方式加载:"+_luatable.num1 + "  " + _luatable.num2+"  "+_luatable.boolValue+"  "+_luatable.num3+" "+_luatable.str); //lua table中常用类型打印
        _luatable.func(); //lua table 中lua函数的执行


        //访问lua表方式二：映射到一个interface,这种方式依赖于生成代码，也就是说你需要使用xlua-clear,然后再使用生成。需要注意使用这种方式用的是引用
        //方式的映射，也就是说在C#端修改相关值，lua对应的值也会跟着改变，本例中的interface：IGetLuaTable
        IGetLuaTable _getLuaTable = luaEnv.Global.Get<IGetLuaTable>("tab");
        Debug.Log("使用普通interface的方式加载:"+_getLuaTable.num1 + "  " + _getLuaTable.num2 + "  " + _getLuaTable.boolValue + "  " + _getLuaTable.num3 + " " + _getLuaTable.str); //lua table中常用类型打印
        _getLuaTable.func(); //lua table 中lua函数的执行

        //访问lua表方式三，轻量级的方式：映射到Dictionary 和List，使用这个有个前提，tab下key和value的值都是一致的,比如key是int 那个value也必须是int

        //使用dictionary 用此方式只有lua表中每个键值对类型一致就可以
        Dictionary<object, object> luatableDic = luaEnv.Global.Get<Dictionary<object,object>>("dictab");
        Debug.Log("使用普通Dictionary的方式加载:");
        foreach (KeyValuePair<object, object> kvp in luatableDic)
        {
            Debug.Log(kvp.Key + "  " + kvp.Value);
        }
        //使用List 用此方式lua中的所有键值对必须统一是一种类型的，所以下面只会打印lua脚本中int类型的值
        List<object> luatableLst = luaEnv.Global.Get<List<object>>("dictab");
        Debug.Log("使用普通List的方式加载:");
        foreach (var item in luatableLst)
        {
            Debug.Log(item);
        }


        //访问Lua表方式三，使用LuaTable,使用这个的好处是不要生成代码，缺点是比方式2要慢一个数量级
        LuaTable luaTable = luaEnv.Global.Get<LuaTable>("tab");
        var num1 = luaTable.Get<int>("num1");
        Debug.Log("使用LuaTable的方式加载");
        Debug.Log(num1);
        
    }
    //定义一个class有对应luatable的字段的public属性，而是有无参构造函数即可
    //这个地方一定要注意此处定义的字段名字一定要和lua脚本相关表中的字段名字一模一样，不然会报错
    //可以相对lua表少字段或者多字段，但是名字一定要和lua表中字段名字相同，然就使用不了
    public class luaTab
    {
        public int num1;
        public int num2;
        public bool boolValue;
        public float num3;
        public string str;
        public Action func;
    }

    //定义一个interface，记得要加CSharpCallLua的标签
    [CSharpCallLua]
    public interface IGetLuaTable
    {
        int num1 { get; set; }
        int num2 { get; set; }
        bool boolValue { get; set; }
        float num3 { get; set; }
        string str { get; set; }
        Action func { get; set; }
    }
    private void OnDestroy()
    {
        luaEnv.Dispose();
    }
}

