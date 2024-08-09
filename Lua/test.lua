-- 实现深拷贝
function DeepCopy(object)
    local lookup_table = {}
    local function _copy(object)
        if type(object) ~="table" then
            return object
        elseif lookup_table[object]  then
            return lookup_table[object]
        end
        
        local new_table = {}
        lookup_table[object] = new_table
        for key, value in pairs(object) do
            new_table[_copy(key)] = _copy[value]
        end
        return setmetatable(new_table,getmetatable(object))
    end
    return _copy(object)
end

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

--面向对象测试
local printf = function (str, ...)
    return print(string.format(str, ...))
end

---声明 classA
local ClassA = class("ClassA")
ClassA.static = 'Static A'
function ClassA:ctor(a,b)
    self.a = a or 0
    self.b = b or 0
end

function ClassA:print()
    printf("%s,a = %s, b = %d, static = %s",self.__cname,self.a,self.b,self.static)
end

function ClassA:getSum()
    return self.a+self.b
end

--声明 classB，并且继承 ClassA
local ClassB = class("ClassB",ClassA)
function ClassB:ctor(...)
    ClassA.ctor(self, ...)
end

--overwrite
function ClassB:print()
    printf('ClassB overwrite super print')
end

--声明classC 继承 classB
local ClassC = class("ClassC",ClassA)
ClassC.static = 'Static C'

local obja1 = ClassA.new(10,20)
local obja2 = ClassA.new(100,200)
local objb1 = ClassB.new(1,2)
local objc = ClassC.new(3,4)
obja1:print()
obja2:print()
objb1:print()
objc:print()
printf("3+4 = %s",objc:getSum())


---UpValue实例
function Outer()
    local x = 10 -- 'x'是一个局部变量
    local function inner()
        print(x) -- 'x'此时成为一个UpValue
        x=x+1
    end
    return inner
end

local myInner = Outer()
myInner()
myInner()

local function foo()
    local i = 1
    local function bar()
        i = i + 1
        print(i)
        return 111
    end
    return bar
end
local fn = foo()
fn()
print(fn())

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