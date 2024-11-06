# 为什么是Lua
## 优势
### 高效执行速度  
   轻量级的虚拟机，JIT动态编译，与游戏引擎深度集成
   动态语法减少静态检查开销，直接CPU执行
### 强大的可拓展性
    lua采用模块化设计，提供开放解释器接口
### 降低成本的内存管理
    解释器体积小，自动内存管理，高效虚拟机实现，跨平台支持
### 支持热更新
    可以不用发版，直接改逻辑，解释性语言
## 劣势
难写  
没有类型检查

# 需要注意的点
## 基础
1. boolean 类型只有true和false lua把false和nil看作false，其他都是true，包括0  

2. type(nil) = nil是错的，因为type返回的是一个字符串
3. 对数字字符串进行算术操作，会尝试将字符串转为数字
4. 使用#放在字符串前面，计算字符串长度
5. lua的表是一个关联数组，key可以是数字或者字符串
6. lua里表的默认初始索引一般是1
7. lua表不固定长度，有新数据添加时table长度自动增加。没初始化默认为nil
8. lua里主要线程时协同程序，和线程差不多，线程可以有任意多个，协程同一时间只有一个
9. t[i],t.i,对table使用索引访问本质上时类似gettable_event(t,i)的调用
10. 遍历table表 in pairs一定会遍历所有元素，但是并非按照key的排列顺序，而是根据table中key的hash值的顺序来遍历，所以每次结果可能不同
11. 函数可以作为参数传递
12. 根据下标输出数组，如果不存在不会报错会输出nil
13.  **table表，能够实现不同的数据类型如数组字典，讲一个表a赋值给另一个表，指针指向同一个地址。将表设置为nil是断开表的指针，面的值不会消失。**
14.  模块与包
     文件名为module.lua,定义一个名为module的模块
     module = {}，再用module.来声明函数和常量
     require是引用函数
 
## 元表 
1. 用来定义对table或userdata操作方式的表
2. 两个表正常不能相加， 这是通过元表定义如何执行。
3. local mt = {}
    mt._add = function(t1,t2)
      ....
    end
    设置t1的元表为mt
    setmetatable(t1,mt)
    **具体过程先查看t1是否有元表，若有，查看t1元表是否有_add元方法，有则调用，没有看t2,如都没则报错**
### __index 元方法
 当通过键来访问table的时候，如果这个键没有值，那么lua会寻找该table的metatable(假如有的话)中的__index键，如果不存在，返回nil，存在则由__index返回结果
```python
mytable = setmetatable({key1 = "value1"},{
    __index = function(mytable,key)
        if key == "key2" then
            return "metatablevalue"
        else
            return nil
        end
    end
})
print(mytable.key1,mytable.key2)
```
输出结果为 value1  metatablevalue

实例解析:
1. mytable 表赋值为{key1 = "value1"}
2. mytable 设置了元表，元方法为 __index
3. 再mytable表中查找key1，如果找到，返回该元素，找不到则继续
4. 在mytable表中查找key2，如果找到，返回metatablevalue，找不到则继续
5. 判断元表有没有__index方法，如果__index方法是一个函数，则调用该函数
6. 元方法中查看是否传入"key2"键的参数，如果传入返回"metatablevalue"，否则返回mytable对应的键值
### __newindex 元方法
__newindex 元方法用来对表更新，__index用来对表访问。
当你给表的一个缺少的索引赋值，解释器会查找到__newindex元方法，如果存在则调用这个函数而不进行赋值操作

## 协程
[协同程序文档](https://www.runoob.com/lua/lua-coroutine.html)