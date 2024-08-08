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
