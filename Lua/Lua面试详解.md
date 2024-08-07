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
```python
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
```python
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
```python
--声明一个 lua class
-- className是类名
-- super为父类
local function class(className，super)--构建类
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