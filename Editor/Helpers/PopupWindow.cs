using System.Linq;

namespace ME.GD.Editor {
	
	using System;
	using UnityEditor;
	using UnityEngine;

    public class PopupWindowAnim : EditorWindow {

		private const float defaultWidth = 150;
		private const float defaultHeight = 250;
		private const float elementHeight = 20;
		
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect;
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator = '/';
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch = true;
		
		/// <summary> Название рута </summary>
		public new string title = "Menu";

		public new string name { get { return title; } set { title = value; } }
		
		/// <summary> Стили, используемые для визуализации попапа </summary>
		private static Styles styles;
		
		//Поиск
		/// <summary> Строка поиска </summary>
		public string searchText = "";

		/// <summary> Активен ли поиск? </summary>
		private bool hasSearch { get { return useSearch && !string.IsNullOrEmpty(searchText); } }
		
		//Анимация
		private float _anim;
		private int _animTarget = 1;
		private long _lastTime;
		
		//Элементы
		/// <summary> Список конечных элементов (до вызова Show) </summary>
		private System.Collections.Generic.List<PopupItem> submenu = new System.Collections.Generic.List<PopupItem>();
		/// <summary> Хранит контекст элементов (нужно при заполнении попапа) </summary>
		private System.Collections.Generic.List<string> folderStack = new System.Collections.Generic.List<string>();
		/// <summary> Список элементов (после вызова Show) </summary>
		private Element[] _tree;
		/// <summary> Список элементов, подходящих под условия поиска </summary>
		private Element[] _treeSearch;
		/// <summary> Хранит контексты элементов (после вызова Show) </summary>
		private System.Collections.Generic.List<GroupElement> _stack = new System.Collections.Generic.List<GroupElement>();
		/// <summary> Указывает, нуждается ли выбор нового элемента в прокрутке </summary>
		private bool scrollToSelected;
		
		private Element[] activeTree { get { return (!hasSearch ? _tree : _treeSearch); } }

		private GroupElement activeParent { get { return _stack[(_stack.Count - 2) + _animTarget]; } }

		public Action onClose;

		private Element activeElement {
			get {
				if (activeTree == null)
					return null;
				var childs = GetChildren(activeTree, activeParent);
				if (childs.Count == 0)
					return null;
				return childs[activeParent.selectedIndex];
			}
		}

		public void OnClose(Action onClose) {

			this.onClose = onClose;

		}

		private void OnDestroy() {
			
			if (this.onClose != null) this.onClose.Invoke();
			
		}

		/// <summary> Создание окна </summary>
		public static PopupWindowAnim Create(Rect screenRect, bool useSearch = true) {
			var popup = CreateInstance<PopupWindowAnim>();
			popup.screenRect = screenRect;
			popup.useSearch = useSearch;
			return popup;
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim CreateByPos(Vector2 pos, bool useSearch = true) {
			return Create(new Rect(pos.x, pos.y, defaultWidth, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна </summary>
		public static PopupWindowAnim CreateByPos(Vector2 pos, float width, bool useSearch = true) {
			return Create(new Rect(pos.x, pos.y, width, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim CreateBySize(Vector2 size, bool useSearch = true) {
			var screenPos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
			return Create(new Rect(screenPos.x, screenPos.y, size.x, size.y), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim Create(float width, bool useSearch = true) {
			return CreateBySize(new Vector2(width, defaultHeight), useSearch);
		}
		
		/// <summary> Создание окна. Вызывается из OnGUI()! </summary>
		public static PopupWindowAnim Create(bool useSearch = true) {
			return CreateBySize(new Vector2(defaultWidth, defaultHeight), useSearch);
		}
		
		/// <summary> Отображает попап </summary>
		public new void Show() {
			if (submenu.Count == 0)
				DestroyImmediate(this);
			else
				Init();
		}
		
		/// <summary> Отображает попап </summary>
		public void ShowAsDropDown() {
			Show();
		}
		
		public void SetHeightByElementCount(int elementCount) {
			screenRect.height = elementCount * elementHeight + (useSearch ? 30f : 0f) + 26f;
		}
		
		public void SetHeightByElementCount() {
			SetHeightByElementCount(maxElementCount);
		}
		
		public bool autoHeight;
		public bool autoClose;
		
		public void BeginRoot(string folderName) {
			var previous = folderStack.Count != 0 ? folderStack[folderStack.Count - 1] : "";
			if (string.IsNullOrEmpty(folderName))
				folderName = "<Noname>";
			if (!string.IsNullOrEmpty(previous))
				folderStack.Add(previous + separator + folderName);
			else
				folderStack.Add(folderName);
		}
		
		public void EndRoot() {
			if (folderStack.Count > 0)
				folderStack.RemoveAt(folderStack.Count - 1);
			else
				throw new Exception("Excess call EndFolder()");
		}
		
		public void EndRootAll() {
			while (folderStack.Count > 0)
				folderStack.RemoveAt(folderStack.Count - 1);
		}
		
		public void Item(string title, Texture2D image, Action action, int order) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image, order = order }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image, order = order });
		}

		public void Item(string title, Texture2D image, Action<PopupItem> action, bool searchable) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image, searchable = searchable }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image, searchable = searchable });
		}

		public void Item(string title, Texture2D image, Action<PopupItem> action, System.Action<Rect> onDraw, bool searchable, int order) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
				            ? new PopupItem(this.title + separator + title, action) { image = image, searchable = searchable, order = order, onDraw = onDraw }
				            : new PopupItem(this.title + separator + folder + separator + title, action) { image = image, searchable = searchable, order = order, onDraw = onDraw });
		}

		public void Item(string title, Action action) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, action)
			            : new PopupItem(this.title + separator + folder + separator + title, action));
		}
		
		public void Item(string title) {
			var folder = "";
			if (folderStack.Count > 0)
				folder = folderStack[folderStack.Count - 1] ?? "";
			submenu.Add(string.IsNullOrEmpty(folder)
			            ? new PopupItem(this.title + separator + title, () => { })
			            : new PopupItem(this.title + separator + folder + separator + title, () => { }));
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, action) { image = image });
		}
		
		public void ItemByPath(string path, Action action) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, action));
		}
		
		public void ItemByPath(string path) {
			if (string.IsNullOrEmpty(path))
				path = "<Noname>";
			submenu.Add(new PopupItem(title + separator + path, () => { }));
		}
		
		private void Init() {
			CreateComponentTree();
			if (autoHeight)
				SetHeightByElementCount();
			ShowAsDropDown(new Rect(screenRect.x, screenRect.y, 1, 1), new Vector2(screenRect.width, screenRect.height));
			Focus();
			wantsMouseMove = true;
		}
		
		private void CreateComponentTree() {

			var list = new System.Collections.Generic.List<string>();
			var elements = new System.Collections.Generic.List<Element>();

			this.submenu = this.submenu.OrderBy(x => x.order).ThenBy(x => x.path).ToList();
			
			for (int i = 0; i < submenu.Count; i++) {

				var submenuItem = submenu[i];
				string menuPath = submenuItem.path;
				var separators = new[] { separator };
				var pathParts = menuPath.Split(separators);

				while (pathParts.Length - 1 < list.Count) {

					list.RemoveAt(list.Count - 1);

				}

				while (list.Count > 0 && pathParts[list.Count - 1] != list[list.Count - 1]) {

					list.RemoveAt(list.Count - 1);

				}

				while (pathParts.Length - 1 > list.Count) {

					elements.Add(new GroupElement(list.Count, pathParts[list.Count]) {
						onDraw = submenuItem.onDraw
					});
					list.Add(pathParts[list.Count]);

				}

				elements.Add(new CallElement(list.Count, pathParts[pathParts.Length - 1], submenuItem) {
					onDraw = submenuItem.onDraw
				});

			}

			_tree = elements.ToArray();
			for (int i = 0; i < _tree.Length; i++) {
				var elChilds = GetChildren(_tree, _tree[i]);
				if (elChilds.Count > maxElementCount)
					maxElementCount = elChilds.Count;
			}
			if (_stack.Count == 0) {
				_stack.Add(_tree[0] as GroupElement);
				goto to_research;
			}
			var parent = _tree[0] as GroupElement;
			var level = 0;
			to_startCycle:
			var stackElement = _stack[level];
			_stack[level] = parent;
			if (_stack[level] != null) {
				_stack[level].selectedIndex = stackElement.selectedIndex;
				_stack[level].scroll = stackElement.scroll;
			}
			level++;
			if (level != _stack.Count) {
				var childs = GetChildren(activeTree, parent);
				var child = childs.FirstOrDefault(x => _stack[level].name == x.name);
				if (child is GroupElement)
					parent = child as GroupElement;
				else
					while (_stack.Count > level)
						_stack.RemoveAt(level);
				goto to_startCycle;
			}
			to_research:
			s_DirtyList = false;
			RebuildSearch();
		}
		
		private int maxElementCount = 1;
		private static bool s_DirtyList = true;

		private void RebuildSearch() {
			if (!hasSearch) {
				_treeSearch = null;
				if (_stack[_stack.Count - 1].name == "Search") {
					_stack.Clear();
					_stack.Add(_tree[0] as GroupElement);
				}
				_animTarget = 1;
				_lastTime = DateTime.Now.Ticks;
			}
			else {
				var separatorSearch = new[] { ' ', separator };
				var searchLowerWords = searchText.ToLower().Split(separatorSearch);
				var firstElements = new System.Collections.Generic.List<Element>();
				var otherElements = new System.Collections.Generic.List<Element>();
				foreach (var element in _tree) {
					if (!(element is CallElement))
						continue;
					if (element.searchable == false) continue;
					var elementNameShortLower = element.name.ToLower().Replace(" ", string.Empty);
					var itsSearchableItem = true;
					var firstContainsFlag = false;
					for (int i = 0; i < searchLowerWords.Length; i++) {
						var searchLowerWord = searchLowerWords[i];
						if (elementNameShortLower.Contains(searchLowerWord)) {
							if (i == 0 && elementNameShortLower.StartsWith(searchLowerWord))
								firstContainsFlag = true;
						}
						else {
							itsSearchableItem = false;
							break;
						}
					}
					if (itsSearchableItem) {
						if (firstContainsFlag)
							firstElements.Add(element);
						else
							otherElements.Add(element);
					}
				}
				firstElements.Sort();
				otherElements.Sort();
				
				var searchElements = new System.Collections.Generic.List<Element>
				{ new GroupElement(0, "Search") };
				searchElements.AddRange(firstElements);
				searchElements.AddRange(otherElements);
				//            searchElements.Add(_tree[_tree.Length - 1]);
				_treeSearch = searchElements.ToArray();
				_stack.Clear();
				_stack.Add(_treeSearch[0] as GroupElement);
				if (GetChildren(activeTree, activeParent).Count >= 1)
					activeParent.selectedIndex = 0;
				else
					activeParent.selectedIndex = -1;
			}
		}
		
		public void OnGUI() {
			if (_tree == null) {
				Close();
				return; 
			}
			//Создание стиля
			if (styles == null)
				styles = new Styles();
			//Фон
			if (s_DirtyList)
				CreateComponentTree();
			HandleKeyboard();
			GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, styles.background);
			
			//Поиск
			if (useSearch) {
				GUILayout.Space(7f);
				var rectSearch = GUILayoutUtility.GetRect(10f, 20f);
				rectSearch.x += 8f;
				rectSearch.width -= 16f;
				EditorGUI.FocusTextInControl("ComponentSearch");
				GUI.SetNextControlName("ComponentSearch");
				if (SearchField(rectSearch, ref searchText))
					RebuildSearch();
			}
			
			//Элементы
			ListGUI(activeTree, _anim, GetElementRelative(0), GetElementRelative(-1));
			if (_anim < 1f && _stack.Count > 1)
				ListGUI(activeTree, _anim + 1f, GetElementRelative(-1), GetElementRelative(-2));
			if (_anim != _animTarget && Event.current.type == EventType.Repaint) {
				var ticks = DateTime.Now.Ticks;
				var coef = (ticks - _lastTime) / 1E+07f;
				_lastTime = ticks;
				_anim = Mathf.MoveTowards(_anim, _animTarget, coef * 4f);
				if (_animTarget == 0 && _anim == 0f) {
					_anim = 1f;
					_animTarget = 1;
					_stack.RemoveAt(_stack.Count - 1);
				}
				Repaint();
			}
		}
		
		private void HandleKeyboard() {
			Event current = Event.current;
			if (current.type == EventType.KeyDown) {
				if (current.keyCode == KeyCode.DownArrow) {
					activeParent.selectedIndex++;
					activeParent.selectedIndex = Mathf.Min(activeParent.selectedIndex,
					                                       GetChildren(activeTree, activeParent).Count - 1);
					scrollToSelected = true;
					current.Use();
				}
				if (current.keyCode == KeyCode.UpArrow) {
					GroupElement element2 = activeParent;
					element2.selectedIndex--;
					activeParent.selectedIndex = Mathf.Max(activeParent.selectedIndex, 0);
					scrollToSelected = true;
					current.Use();
				}
				if (current.keyCode == KeyCode.Return || current.keyCode == KeyCode.KeypadEnter) {
					GoToChild(activeElement, true);
					current.Use();
				}
				if (!hasSearch) {
					if (current.keyCode == KeyCode.LeftArrow || current.keyCode == KeyCode.Backspace) {
						GoToParent();
						current.Use();
					}
					if (current.keyCode == KeyCode.RightArrow) {
						GoToChild(activeElement, false);
						current.Use();
					}
					if (current.keyCode == KeyCode.Escape) {
						Close();
						current.Use();
					}
				}
			}
		}
		
		private static bool SearchField(Rect position, ref string text) {
			var rectField = position;
			rectField.width -= 15f;
			var startText = text;
			text = GUI.TextField(rectField, startText ?? "", styles.searchTextField);
			
			var rectCancel = position;
			rectCancel.x += position.width - 15f;
			rectCancel.width = 15f;
			var styleCancel = text == "" ? styles.searchCancelButtonEmpty : styles.searchCancelButton;
			if (GUI.Button(rectCancel, GUIContent.none, styleCancel) && text != "") {
				text = "";
				GUIUtility.keyboardControl = 0;
			}
			return startText != text;
		}
		
		private void ListGUI(Element[] tree, float anim, GroupElement parent, GroupElement grandParent) {
			anim = Mathf.Floor(anim) + Mathf.SmoothStep(0f, 1f, Mathf.Repeat(anim, 1f));
			Rect rectArea = position;
			rectArea.x = position.width * (1f - anim) + 1f;
			rectArea.y = useSearch ? 30f : 0;
			rectArea.height -= useSearch ? 30f : 0;
			rectArea.width -= 2f;
			GUILayout.BeginArea(rectArea);
			{
				var rectHeader = GUILayoutUtility.GetRect(10f, 25f);
				var nameHeader = parent.name;
				GUI.Label(rectHeader, nameHeader, styles.header);
				if (grandParent != null) {
					var rectHeaderBackArrow = new Rect(rectHeader.x + 4f, rectHeader.y + 7f, 13f, 13f);
					if (Event.current.type == EventType.Repaint)
						styles.leftArrow.Draw(rectHeaderBackArrow, false, false, false, false);
					if (Event.current.type == EventType.MouseDown && rectHeader.Contains(Event.current.mousePosition)) {
						GoToParent();
						Event.current.Use();
					}
				}
				ListGUI(tree, parent);
			}
			GUILayout.EndArea();
		}
		
		private void ListGUI(Element[] tree, GroupElement parent) {
			parent.scroll = GUILayout.BeginScrollView(parent.scroll, new GUILayoutOption[0]);
			EditorGUIUtility.SetIconSize(new Vector2(16f, 16f));
			var children = GetChildren(tree, parent);
			var rect = new Rect();
			for (int i = 0; i < children.Count; i++) {
				var e = children[i];
				var options = new[] { GUILayout.ExpandWidth(true) };
				var rectElement = GUILayoutUtility.GetRect(16f, elementHeight, options);
				if ((Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDown) 
				    && parent.selectedIndex != i && rectElement.Contains(Event.current.mousePosition)) {
					parent.selectedIndex = i;
					Repaint();
				}
				bool on = false;
				if (i == parent.selectedIndex) {
					on = true;
					rect = rectElement;
				}
				if (Event.current.type == EventType.Repaint) {
					if (e.onDraw != null) {
						e.onDraw.Invoke(rectElement);
					} else {
						(e.content.image != null ? styles.componentItem : styles.groupItem).Draw(rectElement, e.content, false, false, on, on);
					}

					if (!(e is CallElement)) {
						var rectElementForwardArrow = new Rect(rectElement.x + rectElement.width - 13f, rectElement.y + 4f, 13f, 13f);
						styles.rightArrow.Draw(rectElementForwardArrow, false, false, false, false);
					}
				}
				if (e.onDraw == null && Event.current.type == EventType.MouseDown && rectElement.Contains(Event.current.mousePosition)) {
					Event.current.Use();
					parent.selectedIndex = i;
					GoToChild(e, true);
				}
			}
			EditorGUIUtility.SetIconSize(Vector2.zero);
			GUILayout.EndScrollView();
			if (scrollToSelected && Event.current.type == EventType.Repaint) {
				scrollToSelected = false;
				var lastRect = GUILayoutUtility.GetLastRect();
				if ((rect.yMax - lastRect.height) > parent.scroll.y) {
					parent.scroll.y = rect.yMax - lastRect.height;
					Repaint();
				}
				if (rect.y < parent.scroll.y) {
					parent.scroll.y = rect.y;
					Repaint();
				}
			}
		}
		
		private void GoToParent() {
			if (_stack.Count <= 1) 
				return;
			_animTarget = 0;
			_lastTime = DateTime.Now.Ticks;
		}
		
		private void GoToChild(Element e, bool addIfComponent) {
			var element = e as CallElement;
			if (element != null) {
				if (!addIfComponent) 
					return;
				element.action();
				if (this.autoClose == true) Close();
			}
			else if (!hasSearch) {
					_lastTime = DateTime.Now.Ticks;
					if (_animTarget == 0)
						_animTarget = 1;
					else if (_anim == 1f) {
							_anim = 0f;
							_stack.Add(e as GroupElement);
						}
				}
		}
		
		private System.Collections.Generic.List<Element> GetChildren(Element[] tree, Element parent) {
			var list = new System.Collections.Generic.List<Element>();
			var num = -1;
			var index = 0;
			while (index < tree.Length) {
				if (tree[index] == parent) {
					num = parent.level + 1;
					index++;
					break;
				}
				index++;
			}
			if (num == -1) 
				return list;
			while (index < tree.Length) {
				var item = tree[index];
				if (item.level < num)
					return list;
				if (item.level <= num || hasSearch)
					list.Add(item);
				index++;
			}
			return list;
		}
		
		private GroupElement GetElementRelative(int rel) {
			int num = (_stack.Count + rel) - 1;
			return num < 0 ? null : _stack[num];
		}
		
		
		private class CallElement : Element {
			public Action action;
			
			public CallElement(int level, string name, PopupItem item) {
				base.level = level;
				content = new GUIContent(name, item.image);
				action = () => {
					item.action();
					content = new GUIContent(name, item.image);
				};
				this.searchable = item.searchable;
			}
		}
		
		[Serializable]
		private class GroupElement : Element {
			public Vector2 scroll;
			public int selectedIndex;
			
			public GroupElement(int level, string name) {
				this.level = level;
				content = new GUIContent(name);
				this.searchable = true;
			}
		}
		
		private class Element : IComparable {
			public GUIContent content;
			public int level;
			public bool searchable;
			public Action<Rect> onDraw;
			
			public string name { get { return content.text; } }
			
			public int CompareTo(object o) {
				return String.Compare(name, ((Element)o).name, StringComparison.Ordinal);
			}
		}
		
		private class Styles {
			public GUIStyle searchTextField = "SearchTextField";
			public GUIStyle searchCancelButton = "SearchCancelButton";
			public GUIStyle searchCancelButtonEmpty = "SearchCancelButtonEmpty";
			public GUIStyle background = "grey_border";
			public GUIStyle componentItem = new GUIStyle("PR Label");
			public GUIStyle groupItem;
			public GUIStyle header = new GUIStyle("In BigTitle");
			public GUIStyle leftArrow = "AC LeftArrow";
			public GUIStyle rightArrow = "AC RightArrow";
			
			public Styles() {
				header.font = EditorStyles.boldLabel.font;
				header.richText = true;
				componentItem.alignment = TextAnchor.MiddleLeft;
				componentItem.padding.left -= 15;
				componentItem.fixedHeight = 20f;
				componentItem.richText = true;
				groupItem = new GUIStyle(componentItem);
				groupItem.padding.left += 0x11;
				groupItem.richText = true;
			}
		}
		
		public class PopupItem {
			public PopupItem(string path, Action action) {
				this.path = path;
				this.action = action;
				this.searchable = true;
			}
			
			public PopupItem(string path, Action<PopupItem> action, Action<Rect> onDraw = null) {
				this.path = path;
				this.onDraw = onDraw;
				this.action = () => { action(this); };
				this.searchable = true;
			}

			public Action<Rect> onDraw;
			public int order;
			public string path;
			public Texture2D image;
			public Action action;
			public bool searchable;

		}
	}
    
    public class Popup {
		/// <summary> Окно, которое связано с попапом </summary>
		internal PopupWindowAnim window;
		/// <summary> Прямоугольник, в котором будет отображен попап </summary>
		public Rect screenRect { get { return window.screenRect; } set { window.screenRect = value; } }
		
		/// <summary> Указывает, что является разделителем в пути </summary>
		public char separator { get { return window.separator; } set { window.separator = value; } }
		
		/// <summary> Позволяет использовать/убирать поиск </summary>
		public bool useSearch { get { return window.useSearch; } set { window.useSearch = value; } }

		/// <summary> Название рута </summary>
		public string title { get { return window.title; } set { window.title = value; } }

		/// <summary> Название рута </summary>
		public string searchText { get { return window.searchText; } set { window.searchText = value; } }

		/// <summary> Автоматически установить размер по высоте, узнав максимальное количество видимых элементов </summary>
		public bool autoHeight { get { return window.autoHeight; } set { window.autoHeight = value; } }
		public bool autoClose { get { return window.autoClose; } set { window.autoClose = value; } }

		/// <summary> Создание окна </summary>
		public Popup(Rect screenRect, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.Create(screenRect, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(Vector2 size, bool useSearch = true, string title = "Menu", char separator = '/') {
			window = PopupWindowAnim.CreateBySize(size, useSearch);
			this.title = title;
			this.separator = separator;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(float width, bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(width, useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		/// <summary> Создание окна </summary>
		public Popup(bool useSearch = true, string title = "Menu", char separator = '/', bool autoHeight = true) {
			window = PopupWindowAnim.Create(useSearch);
			this.title = title;
			this.separator = separator;
			this.autoHeight = autoHeight;
		}
		
		public void BeginFolder(string folderName) {
			window.BeginRoot(folderName);
		}
		
		public void EndFolder() {
			window.EndRoot();
		}
		
		public void EndFolderAll() {
			window.EndRootAll();
		}
		
		public void Item(string name) {
			window.Item(name);
		}
		
		public void Item(string name, Action action, int order = 0) {
			window.Item(name, null, action, order);
		}
		
		public void Item(string name, Texture2D image, Action action, int order = 0) {
			window.Item(name, image, action, order);
		}

		public void Item(string name, Texture2D image, Action<PopupWindowAnim.PopupItem> action, bool searchable = true) {
			window.Item(name, image, action, searchable);
		}

		public void Item(string name, Texture2D image, Action<PopupWindowAnim.PopupItem> action, bool searchable, int order) {
			window.Item(name, image, action, null, searchable, order);
		}

		public void Item(string name, Texture2D image, Action<PopupWindowAnim.PopupItem> action, Action<Rect> onDraw, bool searchable = true, int order = 0) {
			window.Item(name, image, action, onDraw, searchable, order);
		}

		public void ItemByPath(string path) {
			window.ItemByPath(path);
		}
		
		public void ItemByPath(string path, Action action) {
			window.ItemByPath(path, action);
		}
		
		public void ItemByPath(string path, Texture2D image, Action action) {
			window.ItemByPath(path, image, action);
		}
		
		public void Show() {
			window.Show();
		}

		public void OnClose(Action onClose) {

			this.window.OnClose(onClose);

		}
		
		public static void DrawInt(GUIContent label, string selected, System.Action<int> onResult, GUIContent[] options, int[] keys) {
			
			DrawInt_INTERNAL(new Rect(), selected, label, onResult, options, keys, true);
			
		}

		public static void DrawInt(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys) {

			DrawInt_INTERNAL(rect, selected, label, onResult, options, keys, false);

		}

		private static void DrawInt_INTERNAL(Rect rect, string selected, GUIContent label, System.Action<int> onResult, GUIContent[] options, int[] keys, bool layout) {

			var state = false;
			if (layout == true) {

				GUILayout.BeginHorizontal();
				if (label != null) GUILayout.Label(label);
				if (GUILayout.Button(selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				GUILayout.EndHorizontal();

			} else {
				
				if (label != null) rect = EditorGUI.PrefixLabel(rect, label);
				if (GUI.Button(rect, selected, EditorStyles.popup) == true) {
					
					state = true;
					
				}
				
			}
			
			if (state == true) {

				Popup popup = null;
				if (layout == true) {

					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				} else {
					
					Vector2 vector = GUIUtility.GUIToScreenPoint(new Vector2(rect.x, rect.y));
					rect.x = vector.x;
					rect.y = vector.y;
					
					popup = new Popup() { title = (label == null ? string.Empty : label.text), screenRect = new Rect(rect.x, rect.y + rect.height, rect.width, 200f) };
					
				}
				
				for (int i = 0; i < options.Length; ++i) {
					
					var option = options[i];
					var result = keys[i];
					popup.ItemByPath(option.text, () => {
						
						onResult(result);
						
					});
					
				}
				
				popup.Show();

			}

		}

	}
    
}