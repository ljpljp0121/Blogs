## IMGUI

```cpp
	ImGui::CreateContext();
	ImGui_ImplGlfwGL3_Init(window, true);
	ImGui::StyleColorsDark();
```
```cpp
    ImGui_ImplGlfwGL3_NewFrame();
    {
	static float f = 0.0f;
	ImGui::SliderFloat("float", &f, 0.0f, 1.0f);            // Edit 1 float using a slider from 0.0f to 1.0f    
	ImGui::Text("Application average %.3f ms/frame (%.1f FPS)", 1000.0f / ImGui::GetIO().Framerate, ImGui::GetIO().Framerate);
}

ImGui::Render();
ImGui_ImplGlfwGL3_RenderDrawData(ImGui::GetDrawData());
```

```cpp
	ImGui_ImplGlfwGL3_Shutdown();
	ImGui::DestroyContext();
```