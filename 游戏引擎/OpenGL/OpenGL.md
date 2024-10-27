## 抽象OpenGL成类
将索引缓存和顶点缓存抽象成类
那么它就会有bind和unbind方法
```cpp
IndexBuffer::IndexBuffer(const unsigned int* data, unsigned int count) :m_Count(count)
{
	ASSERT(sizeof(unsigned int) == sizeof(GLuint));
	GLCall(glGenBuffers(1, &m_RendererID));
	GLCall(glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_RendererID));
	GLCall(glBufferData(GL_ELEMENT_ARRAY_BUFFER, count * sizeof(unsigned int), data, GL_STATIC_DRAW));
}

IndexBuffer::~IndexBuffer()
{
	GLCall(glDeleteBuffers(1, &m_RendererID));
}

void IndexBuffer::Bind()
{
	GLCall(glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, m_RendererID));
}

void IndexBuffer::Unbind()
{
	GLCall(glBindBuffer(GL_ELEMENT_ARRAY_BUFFER, 0));
}

VertexBuffer::VertexBuffer(const void* data, unsigned int size)
{
	GLCall(glGenBuffers(1, &m_RendererID));
	GLCall(glBindBuffer(GL_ARRAY_BUFFER, m_RendererID));
	GLCall(glBufferData(GL_ARRAY_BUFFER, size, data, GL_STATIC_DRAW));
}

VertexBuffer::~VertexBuffer()
{
	GLCall(glDeleteBuffers(1, &m_RendererID));
}

void VertexBuffer::Bind() const
{
	GLCall(glBindBuffer(GL_ARRAY_BUFFER, m_RendererID));
}

void VertexBuffer::Unbind() const
{
	GLCall(glBindBuffer(GL_ARRAY_BUFFER, 0));
}
```

## 顶点数组
它将顶点缓冲区中无意义的数据和自定义布局联系在一起，使得OpenGL知道该用什么规则去使用这些数据来渲染画面
主要就是要控制好它的布局，我们可以使用push方法将数据push进布局里
```cpp
void VertexArray::AddBuffer(const VertexBuffer& vb, const VertexBufferLayout& layout)
{
	vb.Bind();
	const auto& elements = layout.GetElements();
	unsigned int offset = 0;
	for (unsigned int i = 0; i < elements.size(); i++)
	{
		const auto& element = elements[i];
		GLCall(glEnableVertexAttribArray(i));
		GLCall(glVertexAttribPointer(i, element.count, element.type,
			element.normalized, layout.GetStride(), (const void*)offset));
		offset += element.count * VertexBufferElement::GetSizeOfType(element.type);
	}
}

```
```cpp
struct VertexBufferElement
{
	unsigned int type;
	unsigned int count;
	unsigned char normalized;

	static unsigned int GetSizeOfType(unsigned int type)
	{
		switch (type)
		{
		case GL_FLOAT: return 4;
		case GL_UNSIGNED_INT: return 4;
		case GL_UNSIGNED_BYTE: return 1;
		}
		ASSERT(false);
		return 0;
	}
};

class VertexBufferLayout
{
private:
	std::vector<VertexBufferElement> m_Elements;
	unsigned int m_Stride;
public:
	VertexBufferLayout() :m_Stride(0)
	{

	}

	template<typename T>
	void Push(int count)
	{
		static_assert(false);
	}

	template<>
	void Push <float >(int count)
	{
		m_Elements.push_back({ (GL_FLOAT,count,GL_FALSE) });
		m_Stride += VertexBufferElement::GetSizeOfType(GL_FLOAT);
	}

	template<>
	void Push <unsigned int >(int count)
	{
		m_Elements.push_back({ (GL_UNSIGNED_INT,count,GL_FALSE) });
		m_Stride += VertexBufferElement::GetSizeOfType(GL_UNSIGNED_INT);
	}

	template<>
	void Push <unsigned char >(int count)
	{
		m_Elements.push_back({ (GL_UNSIGNED_BYTE,count,GL_TRUE) });
		m_Stride += VertexBufferElement::GetSizeOfType(GL_UNSIGNED_BYTE);
	}

	inline const std::vector< VertexBufferElement> GetElements() const { return m_Elements; }
	inline unsigned int GetStride() const { return m_Stride; }
};
```

## 着色器抽象
```cpp
Shader shader("res/shader/Basic.shader");
shader.Bind();
shader.SetUniform4f("u_Color", 0.8f, 0.3f, 0.8f, 1.0f);
```
抽象后流程就看着简单很多了
主要就是导入Shader，创建Shader，编译Shader
然后就是设置Uniform了

## Renderer渲染器
渲染需要一个索引缓冲区，顶点数组，当然还有着色器
```cpp
void Renderer::Draw(const VertexArray& va, const IndexBuffer& ib, const Shader& shader) const
{
	va.Bind();
	ib.Bind();
	shader.Bind();
	GLCall(glDrawElements(GL_TRIANGLES, ib.GetCount(), GL_UNSIGNED_INT, nullptr));
}
```