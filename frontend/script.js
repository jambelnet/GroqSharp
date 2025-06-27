const chatWindow = document.getElementById("chatWindow");
const userInput = document.getElementById("userInput");
const sendButton = document.getElementById("sendButton");
const modelSelect = document.getElementById("modelSelect");
const conversationList = document.getElementById("conversationList");
const newSessionBtn = document.getElementById("newSessionBtn");
const baseUri = "https://localhost:7141";

let currentSessionId = generateSessionId();
let currentModel = "";

window.onload = async () => {
  setupThemeToggle();
  await loadModels();
  await fetchConversations();
  await createNewSession(currentSessionId); // initialize first session
};

function setupThemeToggle() {
  const toggleBtn = document.createElement("button");
  toggleBtn.textContent = "🌙";
  toggleBtn.title = "Toggle Light/Dark Mode";
  toggleBtn.style.background = "none";
  toggleBtn.style.border = "none";
  toggleBtn.style.color = "white";
  toggleBtn.style.fontSize = "1.2rem";
  toggleBtn.style.cursor = "pointer";

  document.querySelector("header").appendChild(toggleBtn);
  toggleBtn.onclick = () => {
    document.body.classList.toggle("light-mode");
    toggleBtn.textContent = document.body.classList.contains("light-mode") ? "🌞" : "🌙";
  };
}

async function loadModels() {
  try {
    const response = await fetch(baseUri + "/api/model/list");
    const models = await response.json();

    modelSelect.innerHTML = "";
    models.forEach((model) => {
      const option = document.createElement("option");
      option.value = model;
      option.textContent = model;
      modelSelect.appendChild(option);
    });

    const current = await fetch(baseUri + "/api/model/current");
    currentModel = await current.text();
    modelSelect.value = currentModel;
  } catch (err) {
    console.error("Error loading models:", err);
  }
}

modelSelect.addEventListener("change", async () => {
  const selectedModel = modelSelect.value;
  await fetch(baseUri + "/api/model/set", {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ model: selectedModel })
  });
  currentModel = selectedModel;
});

sendButton.addEventListener("click", sendMessage);
userInput.addEventListener("keypress", (e) => {
  if (e.key === "Enter") sendMessage();
});

newSessionBtn.addEventListener("click", async () => {
  const newId = generateSessionId();
  await createNewSession(newId);
  currentSessionId = newId;
  chatWindow.innerHTML = "";
  await fetchConversations();
});

async function sendMessage() {
  const message = userInput.value.trim();
  if (!message) return;

  addMessage("user", message);
  userInput.value = "";

  try {
    const response = await fetch(`${baseUri}/api/conversations/${currentSessionId}/chat/messages`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ content: message })
    });

    const result = await response.json();
    addMessage("bot", result.response);
  } catch (err) {
    addMessage("bot", "[Error getting response from server]");
    console.error("Chat error:", err);
  }
}

function addMessage(sender, text) {
  const msg = document.createElement("div");
  msg.className = sender === "user" ? "user-message" : "bot-message";
  msg.textContent = text;
  msg.style.opacity = 0;
  chatWindow.appendChild(msg);
  chatWindow.scrollTop = chatWindow.scrollHeight;

  setTimeout(() => {
    msg.style.transition = "opacity 0.3s ease";
    msg.style.opacity = 1;
  }, 10);
}

function generateSessionId() {
  return crypto.randomUUID();
}

async function fetchConversations() {
  try {
    const response = await fetch(`${baseUri}/api/conversations`);
    const conversations = await response.json();
    renderConversationList(conversations);
  } catch (err) {
    console.error("Failed to load conversations:", err);
  }
}

function renderConversationList(conversations) {
  conversationList.innerHTML = "";
  conversations.forEach((conv) => {
    const li = document.createElement("li");
    if (conv.sessionId === currentSessionId) li.classList.add("active");

    const title = document.createElement("span");
    title.className = "convo-title";
    title.textContent = conv.title || conv.sessionId.substring(0, 8);
    title.onclick = async () => {
      currentSessionId = conv.sessionId;
      await loadConversation(conv.sessionId);
      await fetchConversations(); // refresh active highlight
    };

    const actions = document.createElement("div");
    actions.className = "convo-actions";

    const renameBtn = document.createElement("button");
    renameBtn.innerHTML = "✏️";
    renameBtn.onclick = async (e) => {
      e.stopPropagation();
      const newTitle = prompt("Rename conversation:", conv.title || "Untitled");
      if (newTitle) await renameConversation(conv.sessionId, newTitle);
    };

    const deleteBtn = document.createElement("button");
    deleteBtn.innerHTML = "🗑️";
    deleteBtn.onclick = async (e) => {
      e.stopPropagation();
      if (confirm("Delete this conversation?")) {
        await deleteConversation(conv.sessionId);
      }
    };

    actions.appendChild(renameBtn);
    actions.appendChild(deleteBtn);
    li.appendChild(title);
    li.appendChild(actions);
    conversationList.appendChild(li);
  });
}

async function loadConversation(sessionId) {
  try {
    const response = await fetch(`${baseUri}/api/conversations/${sessionId}/history`);
    const history = await response.json();
    chatWindow.innerHTML = "";
    history.forEach(msg => addMessage(msg.role, msg.content));
    chatWindow.scrollTop = chatWindow.scrollHeight;
  } catch (err) {
    console.error("Failed to load conversation:", err);
  }
}

async function createNewSession(sessionId) {
  try {
    await fetch(`${baseUri}/api/conversations/${sessionId}/new`, {
      method: "POST"
    });
  } catch (err) {
    console.error("Failed to create session:", err);
  }
}

async function renameConversation(sessionId, title) {
  try {
    await fetch(`${baseUri}/api/conversations/${sessionId}/rename?newTitle=${encodeURIComponent(title)}`, {
      method: "POST"
    });
    await fetchConversations();
  } catch (err) {
    console.error("Failed to rename conversation:", err);
  }
}

async function deleteConversation(sessionId) {
  try {
    await fetch(`${baseUri}/api/conversations/${sessionId}`, {
      method: "DELETE" });
    await fetchConversations();
  } catch (err) {
    console.error("Failed to delete conversation:", err);
  }
}
