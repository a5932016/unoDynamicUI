# unoDynamicUI

unoDynamicUI 是一個使用 Uno Platform 建立的跨平台 .NET 專案，重點功能是「根據 JSON 動態生成 UI 表單」。

目前已提供一個簡潔的模板頁流程：
- 主頁貼入 JSON
- 直接彈出 Dialog 或 Popup Page
- 動態生成多種控制項
- 在 ViewModel 內觸發 Confirm/Cancel
- 將使用者輸入保存為 JSON

## 功能重點

- Uno Single Project：同一份 UI 程式碼支援多平台
- JSON 動態表單生成（不需要手寫固定欄位 XAML）
- 支援多種欄位類型：
	- input
	- textarea
	- number
	- select
	- checkbox
	- date
	- filePicker
- 欄位驗證：required、min/max、minLength/maxLength、regex、副檔名檢查
- Confirm/Cancel 事件由單一 ViewModel 處理

## 技術堆疊

- .NET + C#
- Uno Platform (Uno.Sdk 6.5.31)
- CommunityToolkit.Mvvm
- Uno.Extensions (Navigation / Hosting / Logging)

## 專案結構

- [unoDynamicUI.slnx](unoDynamicUI.slnx): 解決方案入口
- [unoDynamicUI/unoDynamicUI.csproj](unoDynamicUI/unoDynamicUI.csproj): 主專案與多目標平台設定
- [unoDynamicUI/App.xaml.cs](unoDynamicUI/App.xaml.cs): App 啟動、DI、路由註冊
- [unoDynamicUI/Presentation/MainPage.xaml](unoDynamicUI/Presentation/MainPage.xaml): 主頁 JSON 輸入與入口按鈕
- [unoDynamicUI/Presentation/MainViewModel.cs](unoDynamicUI/Presentation/MainViewModel.cs): 觸發 Dialog / Popup Page 與預設 JSON
- [unoDynamicUI/Presentation/MainPage.xaml.cs](unoDynamicUI/Presentation/MainPage.xaml.cs): Dialog / Popup Page 彈出流程
- [unoDynamicUI/Presentation/DynamicForms/DynamicTemplatePage.xaml](unoDynamicUI/Presentation/DynamicForms/DynamicTemplatePage.xaml): 動態模板頁容器
- [unoDynamicUI/Presentation/DynamicForms/DynamicTemplatePage.xaml.cs](unoDynamicUI/Presentation/DynamicForms/DynamicTemplatePage.xaml.cs): 動態控制項組裝與 File Picker UI
- [unoDynamicUI/Presentation/DynamicForms/DynamicTemplateFormRenderer.cs](unoDynamicUI/Presentation/DynamicForms/DynamicTemplateFormRenderer.cs): 共用動態表單渲染器
- [unoDynamicUI/Presentation/DynamicForms/DynamicTemplateViewModel.cs](unoDynamicUI/Presentation/DynamicForms/DynamicTemplateViewModel.cs): 單一 ViewModel（解析 JSON、驗證、Confirm/Cancel、存值）

## 環境需求

- .NET SDK（建議安裝最新穩定版，至少可支援 net9.0 目標）
- 可執行 Uno Platform 專案的開發環境
- Windows 開發建議使用 Visual Studio 2022 或 VS Code + .NET 工具鏈

補充：
- Uno SDK 版本由 [global.json](global.json) 的 msbuild-sdks 管理。

## 快速開始

1. 還原套件

~~~bash
dotnet restore
~~~

2. 建置 Desktop 目標

~~~bash
dotnet build unoDynamicUI/unoDynamicUI.csproj -f net9.0-desktop
~~~

3. 建置 WebAssembly 目標

~~~bash
dotnet build unoDynamicUI/unoDynamicUI.csproj -f net9.0-browserwasm
~~~

4. 發佈（可選）

~~~bash
dotnet publish unoDynamicUI/unoDynamicUI.csproj -f net9.0-desktop
dotnet publish unoDynamicUI/unoDynamicUI.csproj -f net9.0-browserwasm
~~~

## 使用方式（JSON 動態模板頁）

1. 啟動 App，進入主頁。
2. 在主頁的 Dynamic template JSON 文字區貼上或修改 JSON。
3. 點擊 Open Dialog (From JSON) 或 Open Popup Page (From JSON)。
4. 若使用 Popup Page，可在頁面內點擊 Reload JSON 重新生成欄位。
5. 填寫表單後點擊 Confirm：
	 - 驗證通過時，結果會寫入 SavedValuesJson。
	 - ViewModel 會觸發 Confirmed 事件。
6. 點擊 Cancel：
	 - ViewModel 會觸發 Cancelled 事件。

## JSON 格式定義

根物件：
- title: string
- fields: array

fields 內每個欄位可使用屬性：

| 屬性 | 型別 | 必填 | 說明 |
|---|---|---|---|
| key | string | 是 | 欄位唯一鍵值 |
| label | string | 否 | 顯示名稱 |
| type | string | 否 | 欄位類型，預設 input |
| placeholder | string | 否 | 提示字 |
| required | bool | 否 | 是否必填 |
| defaultValue | string | 否 | 初始值 |
| minLength | int | 否 | 文字最短長度 |
| maxLength | int | 否 | 文字最長長度 |
| min | number | 否 | number 最小值 |
| max | number | 否 | number 最大值 |
| regex | string | 否 | 文字正則驗證 |
| options | string[] | 否 | select 選項 |
| accept | string[] | 否 | filePicker 可接受副檔名（例如 .pdf） |

支援的 type：
- input
- textarea
- number
- select
- checkbox
- date
- filePicker

## JSON 範例

~~~json
{
	"title": "Employee Profile Template",
	"fields": [
		{
			"key": "employeeName",
			"label": "Employee Name",
			"type": "input",
			"placeholder": "Type employee name",
			"required": true,
			"minLength": 2,
			"maxLength": 40
		},
		{
			"key": "department",
			"label": "Department",
			"type": "select",
			"required": true,
			"options": ["Engineering", "Design", "HR", "Finance"]
		},
		{
			"key": "salary",
			"label": "Monthly Salary",
			"type": "number",
			"placeholder": "30000 to 200000",
			"required": true,
			"min": 30000,
			"max": 200000
		},
		{
			"key": "startDate",
			"label": "Start Date",
			"type": "date",
			"required": true
		},
		{
			"key": "isRemote",
			"label": "Remote Work",
			"type": "checkbox"
		},
		{
			"key": "memo",
			"label": "Memo",
			"type": "textarea",
			"placeholder": "Anything you want to mention",
			"maxLength": 200
		},
		{
			"key": "resume",
			"label": "Resume File",
			"type": "filePicker",
			"required": true,
			"accept": [".pdf", ".docx"]
		}
	]
}
~~~

## 動態頁行為說明

- 重新產生欄位：LoadFromJsonCommand
- 確認提交：ConfirmCommand
	- 全欄位驗證
	- 轉型後結果保存到 SavedValues 與 SavedValuesJson
	- 觸發 Confirmed
- 取消：CancelCommand
	- 更新狀態
	- 觸發 Cancelled

## 疑難排解

1. 建置失敗，出現找不到 dotnet.dll

可能訊息：
- Found dotnet SDK, but did not find dotnet.dll at ...

建議處理：
- 重新安裝對應版本 .NET SDK
- 確認 PATH 指向正確 SDK 位置
- 執行 dotnet --info 檢查 SDK 是否完整

2. File Picker 在部分目標平台不可用或拋例外

- 目前程式已捕捉例外並回填狀態訊息
- 需依目標平台補足檔案權限或平台特定初始化設定

## 後續可擴充方向

- 欄位群組與區塊化版型
- 條件顯示（visibleWhen）
- 非同步資料來源下拉選單
- JSON Schema 驗證與版本化

