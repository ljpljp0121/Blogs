### pairs 和ipairs区别
正常情况下是没有区别的    
但是如果自定义key值    
ipairs还是会从1开i始遍历，不能返回nil，仅仅能够返回数字0，遇到nil则直接退出循环  
pairs 能够遍历表中全部的key，并且能除了迭代器本身以及遍历表本身还能够返回nil

获取table长度无论使用#还是table.getn 都会在索引终端的地方停止计数，所以求长度只能够使用 pairs遍历table求长度

### 函数冒号和点的区别
冒号定义方法  默认会接受self参数    
而点号定义 默认不会接受    
冒号定义不会报错
```lua
function class:test()
    print(self.a,self.b)
end
```
点号定义会报错
其实class.test(class) 等价于 class:test()

### 可变参数和多返回值
lua函数可以接受可变数目的参数 在参数列表中使用三点(...)表示参数可变   
固定加可变参数也可以 (n,...) 

lua函数在return后列出想要返回的值的列表，即可返回多值

### 如何对table新建值时提示错误
主要考察__newindex的活学活用，无非元表那点事，遇到问题，先想一想元表能不能解决。
```lua
local table = setmetatable({},{})
table.key = "iam key"
table.value = 123
print(table.key)
//插入以下代码
local mt = getmetatable(table)
function mt:__newindex(key,value)
    table[key] = nil
    print("不能创建新值" .. key)
end
```

### 如何实现深拷贝
使用=运算符进行浅拷贝
1. 拷贝对象时table表   
两者占用同一个对象，两者任意改变都会影响对象
2. 拷贝基础类型，两者改变互不影响
直接遍历将表中元素逐个添加到另一个表中，在将元表设置为原表的元表

### Lua实现Class面向对象
```lua
--实现面向对象
--声明一个 lua class
-- className是类名
-- super为父类
local function class(className,super)
    --构建类
    local clazz = { __cname = className,super = super }
    if super then
        --设置类的元表，此类中没有的，可以查找父类是否含有
        setmetatable(clazz,{ __index = super})
    end
    --new 方法创建对象
    clazz.new = function(...)
        --构造一个对象
        local instance = {}
        --设置对象的元表为当前类，这样，对象就可以调用当前类生命的方法了
        setmetatable(instance,{ __index = clazz})
        if clazz.ctor then
            clazz.ctor(instance, ...)
        end
        return instance
    end
    return clazz
end
```

### 什么是UpValue
1. 一个upvalue有两种状态: open和closed。当一个upvalue被创建时，它是open的，并且它的指针指向Lua栈中对应的变量。当Lua关闭了一个upvalue，upvalue指向的值被复制到upvalue结构内部，并且指针也相应进行调整
2. UpValue指的是一个函数内部所引用的外部局部变量。
3. 当一个函数引用了它外部作用域中的局部变量时，这个局部变量就是一个UpValue，即使外部函数的执行已经结束，引用了这个局部变量的内部函数仍然可以访问和修改这个变量的值
#### 共享
如果两个或多个闭包引用了同一个外部局部变量，它们实际上是共享同一个UpValue。
#### 关闭和开启
当一个闭包被创建时，lua会“开启”(open)这些UpValue，确保这些变量可以被闭包访问。当没有任何闭包引用这些UpValue时，Lua会“关闭”(close)它们。
#### 存储细节
在lua中，会生成一个全局栈，所有的upvalue都会指向该栈中的值，若对应的参数离开的作用域，栈中的值也会被释放，upvalue的指针会指向自己，等待被gc   
闭包运行时，会通过创建指向upvalue的指针，并循环upvalue linked list，找到所需要的外部变量进行运行

### 说说Lua中的闭包
闭包=函数+引用环境
子函数可以使用父函数中的局部变量，这种行为可以理解为闭包  
如果按照闭包的定义来看，Lua的所有函数实际上都是闭包，即使你没有嵌套。这是因为Lua编译器会把Lua 脚本外面，再包装一层主函数。
#### 闭包总结
闭包的主要作用有两个，一是简洁，不需要在不使用时生成对象，也不需要函数名;二是捕获外部变量形成不同的调用环境
#### 闭包原理概述
闭包(函数)编译时会生成原型(prototype)，包含参数、调试信息、虚拟机指令等一系列该闭包的源信息，其中在递归编辑内层函数时，会为内层函数生成指令，同时为该内层函数需要的所有upvalue创建表，以便之后调用时进行upvalue值搜索
在lua中，会生成一个全局栈，所有的upvalue都会指向该栈中的值，若对应的参数离开的作用域，栈中的值也会被释放，upvalue的指针会指向自己，等待被gc
闭包运行时，会通过创建指向upvalue的指针，并循环upvalue linked list，找到所需要的外部变量进行运行

### Lua内存管理机制(GC机制)
采用标记-清除算法，
Lua的内存管理机制通过垃圾回收器和分代回收的方式，自动管理内存的分配和释放。
#### 对象分配
当需要分配内存来存储对象时，他会使用自己的内存分配器。Lua内存分配器基于固定大小的内存块，这些内存块被称为内存块列表。当程序需要分配一个对象时，内存分配器会从内存块列表中选择一个合适的内存块来存储该对象。
#### 垃圾回收器
Lua垃圾回收器周期性运行，以检测并清除不再被引用的对象。有两个阶段：标记阶段和清楚阶段
##### 标记阶段
垃圾回收器从一个称为“根集”的地方开始，逐步遍历所有可达的对象，并将它们标记为活动对象。根集包括全局变量、当前执行的函数中的局部变量以及C函数中的一些特殊数据结构。
##### 清除阶段
再标记阶段完成后，垃圾回收器扫描所有对象，并清除那些没有被标记的对象，这些未被标记的对象被认为不再被引用。
##### 分代回收
Lua的垃圾回收器采用分代回收的策略，它将内存对象分为几代，每一代有不同的优先级。新分配的对象通常被放置在第一代中，而较老的对象会逐渐晋升到更高的代。这种分代机制可以提高垃圾回收的效率，因为新分配的对象通常具有较短的生命周期。
##### 手动内存管理
Lua提供了手动管理内存的机制。其中一个时collectgarbage函数，它可以手动触发垃圾回收过程。通过调整collectgarbage函数的参数，可以对垃圾回收器的行为进行一些控制，例如设置不同的垃圾回收阈值、关闭垃圾回收。此外，lua还提供了weak table来帮助管理对象之间的引用关系，以避免循环引用导致内存泄漏

### C#与Lua的交互细节与优化

#### 1.从致命的gameobj.transform.position = pos开始说起
像gameobj.transform.position = pos这样的写法，在unity中是再常见不过的事情   
但是在ulua中，大量使用这种写法是非常糟糕的。为什么呢?
因为短短一行代码，却发生了非常非常多的事情，为了更直观一点，我们把这行代码调用过的关键luaapil以及ulua相关的关键步骤列出来（以ulua+cstolua导出为准,gameobj是GameObject类型，pos是Vector3) :

##### 第一步:
GameObjectWrap.get_transform lua想从gameobj拿到transform，对应gameobj.transform    
LuaDLL.luanet_rawnetobj把lua中的gameobj变成c#可以辨认的id
ObjectTranslator.TryGetValue用这个id，从ObjectTranslator中获取c#的gameobject对象   
gameobject.transform准备这么多，这里终于真正执行c#获取gameobject.transform了 
ObjectTranslator.AddObject给transform分配一个id，这个id会在lua中用来代表这个transform,transform要保存到ObjectTranslator供未来查找
LuaDLL.luanet_newudata在lua分配一个userdata，把id存进去，用来表示即将返回给lua的transform   
LuaDLL.lua_setmetatable给这个userdata附上metatable，让你可以transform.position这样使用它  
LuaDLL.lua_pushvalue返回transform，后面做些收尾
LuaDLL.lua_rawseti
LuaDLL.lua_remove

##### 第二步：
TransformWrap.set_position lua想把pos设置到transform.positionLuaDLL.luanet_rawnetobj把lua中的transform变成c#可以辨认的id
ObjectTranslator.TryGetValue用这个id，从ObjectTranslator中获取c#的transform对象LuaDLL.tolua_getfloat3 从lua中拿到Vector3的3个float值返回给c#
lua_getfield + lua_tonumber 3次拿xyz的值，退栈
lua_pop
transform.position = new Vector3(x,y,z)准备了这么多，终于执行transform.position = pos赋值了
就这么一行代码，竟然做了这么一大堆的事情!如果是c+ +,a.b.c = x这样经过优化后无非就是拿地址然后内存赋值的事。但是在这里，频繁的取值、入栈、c#到lua的类型转换，每一步都是满满的cpu时间，还不考虑中间产生了各种内存分配和后面的GC!
下面我们会逐步说明，其中有一些东西其实是不必要的，可以省略的。我们可以最终把他优化成:lua_isnumber + lua_tonumber 4次，全部完成

#### 2.在Lua中引用C#的object，代价昂贵
从上面的例子可以看到，仅仅想从gameobj拿到一个transform，就已经有很昂贵的代价
c#的object，不能作为指针直接供c操作(其实可以通过GCHandle进行pinning来做到，不过性能如何未测试，而且被pinning的对象无法用gc管理)，因此主流的lua+unity都是用一个id表示c#的对象，在c#中通过dictionary来对应id和object。同时因为有了这个dictionary的引用，也保证了c#的object在lua有引用的情况下不会被垃圾回收掉。
因此，每次参数中带有object，要从lua中的id表示转换回c#的object，就要做一次dictionary查找;每次调用一个object的成员方法，也要先找到这个object，也就要做dictionary查找。
如果之前这个对象在lua中有用过而且没被gc，那还就是查下dictionary的事情。但如果发现是一个新的在lua中没用过的对象，那就是上面例子中那—大串的准备工作了。
如果你返回的对象只是临时在lua中用一下，情况更糟糕!刚分配的userdata和dictionary索引可能会因为lua的引用被gc而删除掉，然后下次你用到这个对象又得再次做各种准备工作，导致反复的分配和gc，性能很差。
例子中的gameobj.transform就是一个巨大的陷阱，因为.transform只是临时返回一下，但是你后面根本没引用，又会很快被lua释放掉，导致你后面每次.transform一次，都可能意味着一次分配和gc。

#### 3.在Lua和C#间传递unity独有的值类型(Vector3/Quaternion等)更加昂贵
既然前面说了lua调用c#对象缓慢，如果每次vector3.x都要经过c#，那性能基本上就处于崩溃了，所以主流的方案都将Vector3等类型实现为纯lua代码，Vector3就是一个{x,y.z}的table，这样在lua中使用就快了。
但是这样做之后，c#和lua中对Vector3的表示就完全是两个东西了，所以传参就涉及到lua类型和c#类型的转换，例如c#将Vector3传给lua，整个流程如下:
1.c#中拿到Vector3的x,y,z三个值
2.push这3个float给lua栈
3.然后构造一个表，将表的x,y,z赋值
4.将这个表push到返回值里
一个简单的传参就要完成3次push参数、表内存分配、3次表插入，性能可想而知。
那么如何优化呢?我们的测试表明，直接在函数中传递三个float，要比传递Vector3要更快。
例如void SetPos(GameObject obj, Vector3 pos)改为void SetPos(GameObject obj, float x, float y,float z)具体效果可以看后面的测试数据，提升十分明显。

### 对于以上性能问题的解决办法
#### 4.Lua和C#之间传参、返回时，尽可能不要传递以下类型：
严重类: Vector3/Quaternion等unity值类型，数组
次严重类: bool string 各种object
建议传递: int float double
虽然是lua和c#的传参，但是从传参这个角度讲，lua和c#中间其实还夹着一层c(毕竟lua本身也是c实现的)，lua、c、c#由于在很多数据类型的表示以及内存分配策略都不同，因此这些数据在三者间传递，往往需要进行转换(术语parameter mashalling)，这个转换消耗根据不同的类型会有很大的不同

先说次严重类中的boolstring类型，涉及到c和c#的交互性能消耗，根据微软官方文档，在数据类型的处理上，c#定义了Blittable Types和Non-BlittableTypes，其中bool和string属于Non-Blittable Types，意思是他们在c和c#中的内存表示不一样，意味着从c传递到c#时需要进行类型转换，降低性能，而string还要考虑内存分配(将string的内存复制到托管堆，以及utf8和utf16互转)

而严重类，基本上是ulua等方案在尝试lua对象与c#对象对应时的瓶颈所致
Vector3等值类型的消耗，前面已经有所提及而数组则更甚，因为lua中的数组只能以table表示，这和c#下完全是两码事，没有直接的对应关系，因此从c#的数组转换为lua table只能逐个复制，如果涉及object/string等，更是要逐个转换。

#### 5.频繁调用的函数，参数的数量要控制
无论是lua的pushint/checkint，还是c到c#的参数传递，参数转换都是最主要的消耗，而且是逐个参数进行的，因此，lua调用c#的性能，除了跟参数类型相关
外，也跟参数个数有很大关系。一般而言，频繁调用的函数不要超过4个参数，而动辄十几个参数的函数如果频繁调用，你会看到很明显的性能下降，手机上可能一顿调用数百次就可以看到10ms级别的时间。

#### 6.优先使用static函数导出，减少使用成员方法导出
前面提到，一个object要访问成员方法或者成员变量，都需要查找lua userdata和c#对象的引用，或者查找metatable，耗时甚多。直接导出static函数，可以减少这样的消耗。
像obj.transform.position = pos。
我们建议的方法是，写成静态导出函数，类似
```csharp
class LuaUtilf
{
    static void SetPos(GameObject obj, float x, float y, float z)
        {obj.transform.position = new Vector3(x, y, z); }
}
```
然后在lua中LuaUtil.SetPos(obj,pos.x, pos.y, pos.z)，这样的性能会好非常多，因为省掉了transform的频繁返回，而且还避免了transform经常临时返回引起lua的gc。

#### 7.注意lua拿着c#对象的引用时会造成c#对象无法释放，这是内存泄漏常见的起因
前面说到，c# object返回给lua，是通过dictionary将lua的userdata和c# object关联起来，只要lua中的userdata没回收，c# object也就会被这个dictionary拿着引用，导
致无法回收
最常见的就是gameobject和component，如果lua里头引用了他们，即使你进行了Destroy，也会发现他们还残留在mono堆里
不过，因为这个dictionary是lua跟c#的唯一关联，所以要发现这个问题也并不难，遍历一下这个dictionary就很容易发现。ulua下这个dictionary在ObjectTranslator类、slua则在ObjectCache类

#### 8.考虑在lua中只使用自己管理的id，而不直接引用c#的object
想避免lua引用c# object带来的各种性能问题的其中一个方法就是自己分配id去索引object，同时相关c#导出函数不再传递object做参数，而是传递int。
这带来几个好处:
1. 函数调用的性能更好
2. 明确地管理这些object的生命周期，避免让ulua自动管理这些对象的引用，如果在lua中错误地引用了这些对象会导致对象无法释放，从而内存泄露
3. c#object返回到lua中，如果lua没有引用，又会很容易马上gc，并且删除ObjectTranslator对object的引用。自行管理这个引用关系，就不会频繁发生这样的gc行为和分配行为。
例如，上面的LuaUtil.SetPos(GameObject obj, float x, float y, float z)可以进一步优化为LuaUtil.SetPos(int objlD, float x, float y, float z)。然后我们在自己的代码里头记录objID跟GameObject的对应关系，如果可以，用数组来记录而不是dictionary，则会有更快的查找效率。如此下来可以进一步省掉lua调用c#的时间，并目对象的管理也会更高效。

#### 9.合理利用out关键字返回复杂的返回值
在c#向lua返回各种类型的东西跟传参类似，也是有各种消耗的。
比如
```csharp
Vector3 GetPos(GameObject obj)
```
可以写成
```csharp
void GetPos(GameObject obj, out float x, out float y, out float z)
```
表面上参数个数增多了，但是根据生成出来的导出代码(我们以ulua为准)，会从:
LuaDLL.tolua getfloat3 (内含get field + tonumber 3次)
成
isnumber +tonumber 3次
get field本质上是表查找，肯定比isnumber访问栈更慢，因此这样做会有更好的性能。

### 多人开发如何避免全局变量泛滥
_G: 指向全局table
设置一下_G的元表和元方法，通过重写__newindex和__index元方法的方式来做到禁止新建全局变量和访问不存在的全局变量时提示错误。
```lua
setmetatable(
    _G,
    {
        newindex = function( _, key)
            print("attempt to add a new value to global, key: ".. key)
        end,
        _index = function( _, key)
            print("attempt to index a global value, key: " .. key)
        end
    }
)
```

### 热更新原理是什么？
##### 第一种:
lua中的require会阻止多次加载相同的模块。所以当需要更新系统的时候，要卸载掉响应的模块。 (把package.loaded里对应模块名下设置为nil，以保证下次require重新加载)并把全局表中的对应的模块表置 nil。同时把数据记录在专用的全局表下，并用local 去引用它。
初始化这些数据的时候，首先应该检查他们是否被初始化过了。这样来保证数据不被更新过程重置。
```lua
function reloadUp(module name)
    package.loaded[modulename] = nil
    require(modulename)
end
```
这种做法简单粗暴，虽然能完成热更新，但是 问题很多，旧的引用的模块无法得到更新，这种程度的热更新显然不能满足现在的游戏开发需求。
##### 第二种:
```lua
--定义热更新函数，参数为模块名
function reloadUp(module_name)
    --从全局环境 G中获取旧的模块表
    local old_module = _G[module_name]
    -- 将package.loaded中对应模块标记为未加载，准备重新加载
    package.loaded[module_name] = nil
    -- 重新加载模块，实际上是执行了模块的文件，加载新的内容到内存
    require(module_name)
    -- 从全局环境 G中获取刚加载的新模块表
    local new_module = G[module_name]
    -- 遍历新模块表中的所有字段
    for k，v in pairs(new_module) do
        --将新模块表中的字段复制到旧模块表中，实现更新
        old_module[k] = v
    end
    --将更新后的旧模块表重新放回package.loaded，表示模块已被加载
    package.loaded[module_name] = old_module
end
```

### 值类型传递为什么有GC，XLua是如何解决的
#### 为什么会产生GC
通常来说，只要堆分配了内存，也就是实例化引用对象，在对象使用完时，就会被GC。所以值类型时从栈上分配的，原则上不会产生GC，但是在lua和C#交互过程中，值传递会因为装箱拆箱产生GC。

#### 如何解决
以MyStruct为例:
```csharp
[GCOptimize]
[LuaCallCSharp]
public struct MysStruct{
    public MyStruct(int p1, int p2)
    {
        a= p1;
        b = p2;
        c=p2;
        e.c = (byte)p1;
    }
    public int a;
    public int b;
    public decimal c;
    public Pedding e;
}
```
看下面的MyStruct的部分生成代码(打上了GCOptimize标签)
```csharp
// XLuaTest.Mystruct生成代码
[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
static int __CreateInstance(RealStatePtr L)
{
    try
    {
    ObjectTranslator.translator =ObjectTranslatorPool.Instance.Find(L);
    if (LuaAPI.lua_gettop(L) == 3 && LuaTypes.LUA_TNUMBER == LuaAPI.lua_type(L, 2) && LuaTypes.LUA_TNUMBER ==LuaAPI.lua_type(L，3))
        {
            int _p1 = LuaAPI.xlua_tointeger(L，2);
            int _p2 = LuaAPI.xlua_tointeger(L，3);
            var gen_ret = new XLuaTest.MyStruct(_p1,_p2);
            // 无[GCOptimize]标签
            // translator.Push(L，gen_ret);
            // 有[GCOptimize]标签
            translator.PushXLuaTestMyStruct(L，gen_ret);
            return 1;
        }
        if(LuaAPI.lua_gettop(L) == 1)
        {
            // 无[GCOptimize]标签
            // translator.Push(L，default(XLuaTest.MyStruct));
            // 有[GCOptimize]标签
            translator.PushXLuaTestMyStruct(L，default(XLuaTest.MyStruct));
            return 1;
        }
    }
    catch (System.Exception gen_e)
    {
        return LuaAPI.luaL_error(L，"c# exception:" + gen_e);
    }
    return LuaAPI.lual_errorL, "invalid arguments to XLuaTest.MyStruct constructor!");
}
```
如果没有打[GCOptimize] 标签，则translator. PushXLuaTestMyStruct(RealStatePtr L,XLuaTest.MyStruct val)替换为translator.Push(RealStatePtr L, object o)。
可以看出，通用的Push方法的参数是object，这里就会触发装箱的操作。

### 什么是弱引用
我们都知道，lua具有自动内存管理，我们只管创建对象，无需删除对象，对于不再需要的对象只需要简单置为nil,   
lua会自动删除那些被认为是垃圾的数据，问题就在于，什么对象才是垃圾对象呢?有时候，程序员很清楚的知道某个对象是垃圾，而lua却无法发现;
```lua
t={};

--使用一个table作为t的key值
key1 = {name ="key1"};
t[key1] = 1;
key1 = nil;

--又使用一个table作为t的key值
key2 = (name ="key2"};
t[key2] = 1;
key2 = nil;

for key, value in pairs(t) do
    print(key.name .. ":" .. value);
end
-- 强制进行一次垃圾收集
collectgarbage();
for key,value in pairs(t) do
    print(key.name .. ":" .. value);
end
```
代码首先创建了一个空表t，接着创建了一个表key1，然后将key1作为t的键，赋值为1，最后将key1置nil;
key2表也是一样的操作;
接着程序执行了一次collectgarbage0强制进行一次垃圾回收，此时key1和key2表都被置nil，但是并没有被强制回收
换句话说，虽然key1本身为nil，但是他先前指向的内容并没有被删除，因为这个内容被保存在了t中;
那么，如果我们希望在将key1置为nil时，他指向的内容就被清空，不管t中是否引用了这个内容该怎么做?
```lua
t={};
--给t设置一个元表，增加__mode元方法，赋值为"k"
setmetatable(t,{__mode = "k"});

--使用一个table作为t的key值
key1 = {name ="key1"};
t[key1] = 1;
key1 = nil;

--又使用一个table作为t的key值
key2 = (name ="key2"};
t[key2] = 1;
key2 = nil;

for key, value in pairs(t) do
    print(key.name .. ":" .. value);
end
-- 强制进行一次垃圾收集
collectgarbage();
for key,value in pairs(t) do
    print(key.name .. ":" .. value);
end
```
以上代码在创建了表t后，立即将t设置为元表，元表里面有一个 mode字段，值为"k”，通过运行结果，我们知道
在执行collectgarbage()之前，能够输出t中的元素，但是执行垃圾回收之后，就不能再次输出t中的元素的，这是因为
将表设置为元表后，通过 mode ="k”将其指定为对键的弱引用，也就是说，一旦表中的某个键被垃圾回收，t中会删除这个键对应的元素
#### 三种形式的弱引用
lua中提供了以下三种形式的弱引用:
1. key值弱引用，也就是刚刚上面提到的那种形式的弱引用，只要其他地方没有对key值得引用，那么table自身
的这个字段也会被删除，设置方法: setmetatable(t,[__mode ="k"});
2. value值弱引用，类似的，只要其他地方没有对value值得引用，table的这个value所在的字段也会被删除,
设置方法: setmetatable(t,{__mode ="v”});
3. key和value弱引用，规则一样，只要key或者value中一个在其他地方没有引用，table中对应的字段就被
删除，设置方法: setmetatable(t,{ mode ="kv"});

### table表的内部数据结构和ReHash
#### 数据分布
table 的存储分为**数组部分**和**哈希表部分**
1. **数组部分** 从1开始作整数数字索引。这可以提供紧凑且高效的随机访问。
2. **哈希表部分** 唯一不能做哈希键值的是nil，这个限制可以帮助我们发现许多运行期错误
#### 数据结构
首先我们了解一下table的数据结构
```c
typedef union TKey {
    struct {
        TValuefields;
        struct Node *next;/* for chaining */
    } nk;
    TValue tvk;
} TKey;

typedef struct Node{
    TValue i_val;
    TKey i_key;
} Node;
// lua table的基本数据结构
typedef struct Table {
    CommonHeader;
    lu_byte flags; /* 1<<p means tagmethod(p) is not present */
    lu_byte lsizenode; /* log2 of size of 'node' array */
    struct Table *metatable;
    TValue *array; /* array part */
    Node *node;/* hash part */
    Node *lastfree; /* any free position is before this position */
    GCObject *gclist;
    int sizearray; /* size of array' array */
}Table;
```
#### ReHash过程
它们俩的内存大小是动态变化的，如果空间不够就需要分配更多的空间，如果空间利用率太少就需要缩减内存，这个过程叫做rehash。
```c
rehash内部，主要是做了以下几件事:
a. 计算array part的key的数量
b. 计算hash part的key的数量
c. 计算新设的key之后array part部分的数量
d. 计算一个新的array part部分需要分配的内存大小
e. resize
```
#### 总结
尽量可以提前分配大小，明确知道table的内容或者知道大小的，可以先预先初始化。例如:
1. 不建议: local tb = {}; tb[1] = 1; tb[2] = 2; tb[3] = 3; 因为这样会多次触发rehash
2. 建议: local tb =nil,nil,nil}，或者local tb ={1,2,3},后面再作赋值操作
3. 注意在使用长度操作符#对数组其长度时，数组不应该包含nil值，否则很容易出错
4. table中要想删除一个元素等同于向对应key赋值为nil，等待垃圾回收。但是删除table一个元素时候
并不会触发表重构行为，即不会触发rehash操作。

### rawset和rawget
在Lua中，rawset和rawget 是两个用于直接操作表(table)的函数，它们绕过了元表(metatable)中定义的元方法(metamethods)。这意味着即使表设置了元表，并且元表中包含了_index和_newindex这样的元方法，使用rawset和rawget也会忽略这些元方法，直接访问或修改表的实际内容。

#### rawset
rawset 函数用于直接在表中设置一个键值对，而不触发元表中的_newindex元方法。其语法如下:
```lua
rawset(table,key,value)
```
1. table是你想要修改的表
2. key是你想要设置的键
3. value是与键关联的值   

如果尝试对一个设置了__newindex元方法的表进行修改，通常元方法会被调用。使用rawset可以跳过这个行为，直接修改表。

#### rawget
rawget 函数用于直接从表中获取键对应的值，而不触发元表中的__index方法。其语法如下:
```lua
value = rawget(table,key)
```
1. table是你想要从中获取的表
2. key是你想要获取值的键

与rawset类似，如果表设置了元表，并且元表中定义了__index元方法，通常访问一个不存在的键会触发这个元方法。使用rawget可以直接从表中获取值，而不会调用__index元方法。
##### 实例
```lua
local mt = {
    __index = function (t,k)
        print("Tried to access the key " .. tostring(k))
    end,
    __newindex = function (t,k,v)
        print("Tried to set " .. tostring(k) .. " to " .. tostring(v))
    end
}

local t = {}
setmetatable(t,mt)

--通常访问和设置会触发元方法
t.a = 1 --输出：Tried to set a to 1
print(t.a) -- 输出：Tried to access the key a

--使用rawset和rawget绕过元方法
rawset(t,"a",2)
print(rawget(t,"a")) --输出：2
```

### Lua调试原理
#### 1.如何让程序暂停执行
Lua虚拟机(也可称之为解释器)内部提供了一个接口:用户可以在应用程序中设置一个钩子函数(Hook)，虚拟机在执行指令码的时候会检查用户是否设置了钩子函数，如果设置了，就调用这个钩子函数。本质上就是设置一个回调函数，因为都是用C语言来实现的，虚拟机中只要把这个钩子函数的地址记住，然后在某些场合回调这个函数就可以了。

设置钩子函数的基础API原型如下:
```c
void lua_sethook(lua_State *L, lua_Hook f, int mask, int count);
```
也可以通过下面即将介绍的调试库中的函数来设置钩子函数，效果是一样的，因为调试库函数的内部也是调用基础函数。
```lua
debug.sethook([thread,] hook, mask [,count])
```

#### 2.Lua调试库是什么？
先看库中提供的几个重要的函数:
```lua
debug.gethook
debug.sethook
debug.getinfo
debug.getlocal
debug.setlocal
debug.setupvalue
debug.traceback
debug.getregistry
```
上面已经说到，Lua给用户提供了设置钩子的API函数lua_sethook,用户可以直接调用这个函数，此时传入的钩子函数的定义格式需要满足要求。

#### 3.获取程序内部信息
在钩子函数中，可以通过如下API函数来获取程序内部的信息:
```c
int lua_getinfo(lua_State *L, const char *what, lua_Debug *ar);
```
在这个API函数中：
第二个参数用来告诉虚拟机我们想要获取程序的哪些信息
第三个参数用来存储获取到的信息

#### 4.修改程序内部信息
经过上面的讲解，已经看到我们获取程序信息都是通过Lua提供的API函数，或者是利用调试库提供的接口函数来完成的。那么修改程序内部信息也同样如此。
Lua提供了下面这2个API函数来修改函数中的变量:
1. 修改当前活动记录总的局部变量的值:
```c
const char *lua_setlocal(lua_State *L, const lua_Debug *ar, int n);
```
2. 设置闭包上值的值(上值upvalue就是闭包使用了外层的那些变量)
```c
const char *lua_setupvalue(lua_State *L, int funcindex, int n);
```
同样的，也可以利用调试库中的debug.setlocal和debug.setupvalue来完成同样的功能。


