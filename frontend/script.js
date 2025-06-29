class ChatApp {
  constructor() {
    this.baseUri = "https://localhost:7141";
    //this.currentSessionId = this.generateSessionId();
    this.currentModel = "";
    this.settings = this.loadSettings();
    this.isTyping = false;
    
    // DOM elements
    this.elements = {
      // Mobile elements
      hamburgerBtn: document.getElementById("hamburgerBtn"),
      sidebarOverlay: document.getElementById("sidebarOverlay"),
      
      // Sidebar elements
      sidebar: document.getElementById("sidebar"),
      newSessionBtn: document.getElementById("newSessionBtn"),
      conversationList: document.getElementById("conversationList"),
      
      // Chat elements
      chatWindow: document.getElementById("chatWindow"),
      userInput: document.getElementById("userInput"),
      sendButton: document.getElementById("sendButton"),
      
      // Header elements
      modelSelect: document.getElementById("modelSelect"),
      themeToggle: document.getElementById("themeToggle"),
      settingsBtn: document.getElementById("settingsBtn"),
      desktopSettingsBtn: document.getElementById("desktopSettingsBtn"),
      
      // Settings modal
      settingsModal: document.getElementById("settingsModal"),
      closeSettings: document.getElementById("closeSettings"),
      themeSelect: document.getElementById("themeSelect"),
      modelSelectSettings: document.getElementById("modelSelectSettings"),
      //autoSave: document.getElementById("autoSave"),
      messageLimit: document.getElementById("messageLimit"),
      resetSettings: document.getElementById("resetSettings"),
      saveSettings: document.getElementById("saveSettings")
    };
    
    this.init();
  }
  
  async init() {
    this.setupEventListeners();
    this.applyTheme();
    await this.loadModels();
    await this.fetchConversations();
    //await this.createNewSession(this.currentSessionId);
    this.setupKeyboardShortcuts();
  }
  
  setupEventListeners() {
    // Mobile navigation
    this.elements.hamburgerBtn?.addEventListener("click", () => this.toggleSidebar());
    this.elements.sidebarOverlay?.addEventListener("click", () => this.closeSidebar());
    
    // Chat functionality
    this.elements.sendButton?.addEventListener("click", () => this.sendMessage());
    this.elements.userInput?.addEventListener("keypress", (e) => {
      if (e.key === "Enter" && !e.shiftKey) {
        e.preventDefault();
        this.sendMessage();
      }
    });
    
    // Auto-resize textarea
    this.elements.userInput?.addEventListener("input", () => this.autoResizeInput());
    
    // Session management
    this.elements.newSessionBtn?.addEventListener("click", () => this.createNewConversation());
    
    // Model selection
    this.elements.modelSelect?.addEventListener("change", () => this.changeModel());
    
    // Theme toggle
    this.elements.themeToggle?.addEventListener("click", () => this.toggleTheme());
    
    // Settings modal
    this.elements.settingsBtn?.addEventListener("click", () => this.openSettings());
    this.elements.desktopSettingsBtn?.addEventListener("click", () => this.openSettings());
    this.elements.closeSettings?.addEventListener("click", () => this.closeSettings());
    this.elements.saveSettings?.addEventListener("click", () => this.saveSettings());
    this.elements.resetSettings?.addEventListener("click", () => this.resetSettings());
    
    // Close modal on overlay click
    this.elements.settingsModal?.addEventListener("click", (e) => {
      if (e.target === this.elements.settingsModal) {
        this.closeSettings();
      }
    });
    
    // Close dropdowns when clicking outside
    document.addEventListener("click", (e) => this.closeAllDropdowns(e));
    
    // Handle window resize
    window.addEventListener("resize", () => this.handleResize());
  }
  
  setupKeyboardShortcuts() {
    document.addEventListener("keydown", (e) => {
      // Ctrl/Cmd + N for new conversation
      if ((e.ctrlKey || e.metaKey) && e.key === "n") {
        e.preventDefault();
        this.createNewConversation();
      }
      
      // Ctrl/Cmd + , for settings
      if ((e.ctrlKey || e.metaKey) && e.key === ",") {
        e.preventDefault();
        this.openSettings();
      }
      
      // Escape to close modals/sidebar
      if (e.key === "Escape") {
        this.closeSettings();
        this.closeSidebar();
        this.closeAllDropdowns();
      }
    });
  }
  
  // Sidebar Management
  toggleSidebar() {
    const isActive = this.elements.sidebar?.classList.contains("active");
    if (isActive) {
      this.closeSidebar();
    } else {
      this.openSidebar();
    }
  }
  
  openSidebar() {
    this.elements.sidebar?.classList.add("active");
    this.elements.sidebarOverlay?.classList.add("active");
    this.elements.hamburgerBtn?.classList.add("active");
    document.body.style.overflow = "hidden";
  }
  
  closeSidebar() {
    this.elements.sidebar?.classList.remove("active");
    this.elements.sidebarOverlay?.classList.remove("active");
    this.elements.hamburgerBtn?.classList.remove("active");
    document.body.style.overflow = "";
  }
  
  handleResize() {
    if (window.innerWidth > 768) {
      this.closeSidebar();
    }
  }
  
  // Theme Management
  applyTheme() {
    const theme = this.settings.theme;
    if (theme === "auto") {
      const prefersDark = window.matchMedia("(prefers-color-scheme: dark)").matches;
      document.documentElement.setAttribute("data-theme", prefersDark ? "dark" : "light");
    } else {
      document.documentElement.setAttribute("data-theme", theme);
    }
    
    // Update theme toggle icon
    /* const isDark = document.documentElement.getAttribute("data-theme") === "dark";
    if (this.elements.themeToggle) {
      this.elements.themeToggle.querySelector(".theme-icon").textContent = isDark ? "🌙" : "🌞";
    } */
  }
  
  toggleTheme() {
    const currentTheme = document.documentElement.getAttribute("data-theme");
    const newTheme = currentTheme === "dark" ? "light" : "dark";
    this.settings.theme = newTheme;
    this.saveSettingsToStorage();
    this.applyTheme();
  }
  
  // Model Management
  async loadModels() {
    try {
      const response = await fetch(`${this.baseUri}/api/model/list`);
      const models = await response.json();
      
      this.populateModelSelect(this.elements.modelSelect, models);
      this.populateModelSelect(this.elements.modelSelectSettings, models);
      
      const current = await fetch(`${this.baseUri}/api/model/current`);
      this.currentModel = await current.text();
      
      if (this.elements.modelSelect) {
        this.elements.modelSelect.value = this.currentModel;
      }
      if (this.elements.modelSelectSettings) {
        this.elements.modelSelectSettings.value = this.currentModel;
      }
    } catch (err) {
      console.error("Error loading models:", err);
      this.showNotification("Failed to load models", "error");
    }
  }
  
  populateModelSelect(selectElement, models) {
    if (!selectElement) return;
    
    selectElement.innerHTML = "";
    models.forEach((model) => {
      const option = document.createElement("option");
      option.value = model;
      option.textContent = model;
      selectElement.appendChild(option);
    });
  }
  
  async changeModel() {
    const selectedModel = this.elements.modelSelect?.value;
    if (!selectedModel) return;
    
    try {
      await fetch(`${this.baseUri}/api/model/set`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ model: selectedModel })
      });
      this.currentModel = selectedModel;
      
      // Sync with settings modal
      if (this.elements.modelSelectSettings) {
        this.elements.modelSelectSettings.value = selectedModel;
      }
    } catch (err) {
      console.error("Error changing model:", err);
      this.showNotification("Failed to change model", "error");
    }
  }
  
  // Chat Functionality
  async sendMessage() {
    const message = this.elements.userInput?.value.trim();
    if (!message || this.isTyping) return;
    
    this.addMessage("user", message);
    this.elements.userInput.value = "";
    this.autoResizeInput();
    this.elements.sendButton.disabled = true;
    
    // Show typing indicator
    this.showTypingIndicator();
    
    try {
      const response = await fetch(`${this.baseUri}/api/conversations/${this.currentSessionId}/chat/messages`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ content: message })
      });
      
      const result = await response.json();
      this.hideTypingIndicator();
      this.addMessage("bot", result.response);
      
      // Auto-save if enabled
      //if (this.settings.autoSave) {
        await this.fetchConversations();
      //}
    } catch (err) {
      this.hideTypingIndicator();
      this.addMessage("bot", "[Error getting response from server]");
      console.error("Chat error:", err);
      this.showNotification("Failed to send message", "error");
    } finally {
      this.elements.sendButton.disabled = false;
    }
  }
  
  addMessage(sender, text) {
    const messageDiv = document.createElement("div");
    messageDiv.className = `message ${sender}-message`;
    messageDiv.textContent = text;
    
    this.elements.chatWindow?.appendChild(messageDiv);
    this.scrollToBottom();
    
    // Limit message history
    this.limitMessageHistory();
  }
  
  showTypingIndicator() {
    this.isTyping = true;
    const typingDiv = document.createElement("div");
    typingDiv.className = "typing-indicator";
    typingDiv.id = "typing-indicator";
    
    const dotsDiv = document.createElement("div");
    dotsDiv.className = "typing-dots";
    
    for (let i = 0; i < 3; i++) {
      const dot = document.createElement("span");
      dotsDiv.appendChild(dot);
    }
    
    typingDiv.appendChild(dotsDiv);
    this.elements.chatWindow?.appendChild(typingDiv);
    this.scrollToBottom();
  }
  
  hideTypingIndicator() {
    this.isTyping = false;
    const typingIndicator = document.getElementById("typing-indicator");
    if (typingIndicator) {
      typingIndicator.remove();
    }
  }
  
  limitMessageHistory() {
    const messages = this.elements.chatWindow?.querySelectorAll(".message");
    const limit = this.settings.messageLimit;
    
    if (messages && messages.length > limit) {
      const excess = messages.length - limit;
      for (let i = 0; i < excess; i++) {
        messages[i].remove();
      }
    }
  }
  
  autoResizeInput() {
    const input = this.elements.userInput;
    if (!input) return;
    
    input.style.height = "auto";
    input.style.height = Math.min(input.scrollHeight, 120) + "px";
  }
  
  scrollToBottom() {
    if (this.elements.chatWindow) {
      this.elements.chatWindow.scrollTop = this.elements.chatWindow.scrollHeight;
    }
  }
  
  // Conversation Management
  async fetchConversations() {
    try {
      const response = await fetch(`${this.baseUri}/api/conversations`);
      const conversations = await response.json();
      this.renderConversationList(conversations);
    } catch (err) {
      console.error("Failed to load conversations:", err);
      this.showNotification("Failed to load conversations", "error");
    }
  }
  
  renderConversationList(conversations) {
    if (!this.elements.conversationList) return;
    
    this.elements.conversationList.innerHTML = "";
    
    conversations.forEach((conv) => {
      const li = document.createElement("li");
      li.className = "conversation-item";
      if (conv.sessionId === this.currentSessionId) {
        li.classList.add("active");
      }
      
      const title = document.createElement("span");
      title.className = "conversation-title";
      title.textContent = conv.title || conv.sessionId.substring(0, 8);
      title.addEventListener("click", () => this.loadConversation(conv.sessionId));
      
      const actions = document.createElement("div");
      actions.className = "conversation-actions";
      
      const menuBtn = document.createElement("button");
      menuBtn.className = "menu-btn";
      menuBtn.setAttribute("aria-label", "Conversation options");
      
      const menuDots = document.createElement("div");
      menuDots.className = "menu-dots";
      for (let i = 0; i < 3; i++) {
        const dot = document.createElement("span");
        menuDots.appendChild(dot);
      }
      menuBtn.appendChild(menuDots);
      
      // Create dropdown menu
      const dropdown = this.createDropdownMenu(conv);
      menuBtn.appendChild(dropdown);
      
      menuBtn.addEventListener("click", (e) => {
        e.stopPropagation();
        this.toggleDropdown(dropdown);
      });
      
      actions.appendChild(menuBtn);
      li.appendChild(title);
      li.appendChild(actions);
      this.elements.conversationList.appendChild(li);
    });
  }
  
  createDropdownMenu(conversation) {
    const dropdown = document.createElement("div");
    dropdown.className = "dropdown-menu";
    
    const renameBtn = document.createElement("button");
    renameBtn.className = "dropdown-item";
    //renameBtn.textContent = "Rename";
	renameBtn.innerHTML = `
	  <svg class="menu-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
		<path d="M12 20h9"/>
		<path d="M16.5 3.5l4 4L7 21H3v-4L16.5 3.5z"/>
	  </svg>
	  Rename
	`;
    renameBtn.addEventListener("click", (e) => {
      e.stopPropagation();
      this.renameConversation(conversation);
      this.closeAllDropdowns();
    });
    
    const deleteBtn = document.createElement("button");
    deleteBtn.className = "dropdown-item";
    //deleteBtn.textContent = "Delete";
	deleteBtn.innerHTML = `
	  <svg class="menu-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
		<polyline points="3 6 5 6 21 6"/>
		<path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6"/>
		<path d="M10 11v6M14 11v6"/>
		<path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2"/>
	  </svg>
	  Delete
	`;
    deleteBtn.addEventListener("click", (e) => {
      e.stopPropagation();
      this.deleteConversation(conversation.sessionId);
      this.closeAllDropdowns();
    });
    
    dropdown.appendChild(renameBtn);
    dropdown.appendChild(deleteBtn);
    
    return dropdown;
  }
  
  toggleDropdown(dropdown) {
    this.closeAllDropdowns();
    dropdown.classList.add("active");
  }
  
  closeAllDropdowns(event) {
    const dropdowns = document.querySelectorAll(".dropdown-menu");
    dropdowns.forEach(dropdown => {
      if (!event || !dropdown.contains(event.target)) {
        dropdown.classList.remove("active");
      }
    });
  }
  
  async loadConversation(sessionId) {
    try {
      this.currentSessionId = sessionId;
      const response = await fetch(`${this.baseUri}/api/conversations/${sessionId}/history`);
      const history = await response.json();
      
      this.elements.chatWindow.innerHTML = "";
      history.forEach(msg => this.addMessage(msg.role, msg.content));
      
      await this.fetchConversations(); // Refresh to update active state
      this.closeSidebar(); // Close sidebar on mobile after selection
    } catch (err) {
      console.error("Failed to load conversation:", err);
      this.showNotification("Failed to load conversation", "error");
    }
  }
  
  async createNewConversation() {
    const newId = this.generateSessionId();
    try {
      await this.createNewSession(newId);
      this.currentSessionId = newId;
      this.elements.chatWindow.innerHTML = "";
      await this.fetchConversations();
      this.closeSidebar(); // Close sidebar on mobile
    } catch (err) {
      console.error("Failed to create new conversation:", err);
      this.showNotification("Failed to create new conversation", "error");
    }
  }
  
  async createNewSession(sessionId) {
    try {
      await fetch(`${this.baseUri}/api/conversations/${sessionId}/new`, {
        method: "POST"
      });
    } catch (err) {
      console.error("Failed to create session:", err);
      throw err;
    }
  }
  
  async renameConversation(conversation) {
    const newTitle = prompt("Rename conversation:", conversation.title || "Untitled");
    if (!newTitle) return;
    
    try {
      await fetch(`${this.baseUri}/api/conversations/${conversation.sessionId}/rename?newTitle=${encodeURIComponent(newTitle)}`, {
        method: "POST"
      });
      await this.fetchConversations();
      this.showNotification("Conversation renamed", "success");
    } catch (err) {
      console.error("Failed to rename conversation:", err);
      this.showNotification("Failed to rename conversation", "error");
    }
  }
  
  async deleteConversation(sessionId) {
    if (!confirm("Delete this conversation? This action cannot be undone.")) return;
    
    try {
      await fetch(`${this.baseUri}/api/conversations/${sessionId}`, {
        method: "DELETE"
      });
      
      // If we deleted the current conversation, create a new one
      if (sessionId === this.currentSessionId) {
        await this.createNewConversation();
      } else {
        await this.fetchConversations();
      }
      
      this.showNotification("Conversation deleted", "success");
    } catch (err) {
      console.error("Failed to delete conversation:", err);
      this.showNotification("Failed to delete conversation", "error");
    }
  }
  
  // Settings Management
  openSettings() {
    this.elements.settingsModal?.classList.add("active");
    this.populateSettingsForm();
    document.body.style.overflow = "hidden";
  }
  
  closeSettings() {
    this.elements.settingsModal?.classList.remove("active");
    document.body.style.overflow = "";
  }
  
  populateSettingsForm() {
    if (this.elements.themeSelect) {
      this.elements.themeSelect.value = this.settings.theme;
    }
    if (this.elements.modelSelectSettings) {
      this.elements.modelSelectSettings.value = this.currentModel;
    }
    /*if (this.elements.autoSave) {
      this.elements.autoSave.checked = this.settings.autoSave;
    }*/
    if (this.elements.messageLimit) {
      this.elements.messageLimit.value = this.settings.messageLimit;
    }
  }
  
  async saveSettings() {
    // Update settings object
    if (this.elements.themeSelect) {
      this.settings.theme = this.elements.themeSelect.value;
    }
    /*if (this.elements.autoSave) {
      this.settings.autoSave = this.elements.autoSave.checked;
    }*/
    if (this.elements.messageLimit) {
      this.settings.messageLimit = parseInt(this.elements.messageLimit.value);
    }
    
    // Change model if different
    const selectedModel = this.elements.modelSelectSettings?.value;
    if (selectedModel && selectedModel !== this.currentModel) {
      try {
        await fetch(`${this.baseUri}/api/model/set`, {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ model: selectedModel })
        });
        this.currentModel = selectedModel;
        
        // Sync with main model select
        if (this.elements.modelSelect) {
          this.elements.modelSelect.value = selectedModel;
        }
      } catch (err) {
        console.error("Error changing model:", err);
        this.showNotification("Failed to change model", "error");
        return;
      }
    }
    
    this.saveSettingsToStorage();
    this.applyTheme();
    this.closeSettings();
    this.showNotification("Settings saved", "success");
  }
  
  resetSettings() {
    if (!confirm("Reset all settings to default? This action cannot be undone.")) return;
    
    this.settings = this.getDefaultSettings();
    this.saveSettingsToStorage();
    this.populateSettingsForm();
    this.applyTheme();
    this.showNotification("Settings reset to default", "success");
  }
  
  loadSettings() {
    const saved = localStorage.getItem("chatAppSettings");
    if (saved) {
      try {
        return { ...this.getDefaultSettings(), ...JSON.parse(saved) };
      } catch (err) {
        console.error("Failed to parse saved settings:", err);
      }
    }
    return this.getDefaultSettings();
  }
  
  saveSettingsToStorage() {
    localStorage.setItem("chatAppSettings", JSON.stringify(this.settings));
  }
  
  getDefaultSettings() {
    return {
      theme: "dark",
      //autoSave: true,
      messageLimit: 100
    };
  }
  
  // Utility Functions
  generateSessionId() {
    return crypto.randomUUID();
  }
  
  showNotification(message, type = "info") {
    // Create a simple notification system
    const notification = document.createElement("div");
    notification.className = `notification notification-${type}`;
    notification.textContent = message;
    notification.style.cssText = `
      position: fixed;
      top: 10px;
      right: 20px;
      padding: 12px 20px;
      border-radius: 8px;
      color: white;
      font-weight: 500;
      z-index: 3000;
      transform: translateX(100%);
      transition: transform 0.3s ease;
      background-color: ${type === "error" ? "#f44336" : type === "success" ? "#4caf50" : "#2196f3"};
    `;
    
    document.body.appendChild(notification);
    
    // Animate in
    setTimeout(() => {
      notification.style.transform = "translateX(0)";
    }, 10);
    
    // Remove after 3 seconds
    setTimeout(() => {
      notification.style.transform = "translateX(100%)";
      setTimeout(() => {
        if (notification.parentNode) {
          notification.parentNode.removeChild(notification);
        }
      }, 300);
    }, 3000);
  }
}

// Initialize the app when DOM is loaded
document.addEventListener("DOMContentLoaded", () => {
  new ChatApp();
});

// Handle system theme changes
window.matchMedia("(prefers-color-scheme: dark)").addEventListener("change", (e) => {
  if (window.chatApp && window.chatApp.settings.theme === "auto") {
    window.chatApp.applyTheme();
  }
});
