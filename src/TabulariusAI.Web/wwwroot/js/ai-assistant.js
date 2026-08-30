(() => {
  const form = document.getElementById('assistantForm');
  if (!form) return;
  const input = document.getElementById('assistantQuestion');
  const messages = document.getElementById('assistantMessages');
  const counter = document.getElementById('assistantCounter');
  const submit = form.querySelector('button[type="submit"]');
  const dossierId = Number(document.getElementById('assistantDossierId')?.value);
  const token = form.querySelector('input[name="__RequestVerificationToken"]')?.value;
  const history = [];

  const escapeHtml = value => value.replace(/[&<>"']/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character]));
  const inlineMarkdown = value => escapeHtml(value)
    .replace(/`([^`]+)`/g, '<code>$1</code>')
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
    .replace(/__([^_]+)__/g, '<strong>$1</strong>')
    .replace(/(?<!\*)\*([^*\n]+)\*(?!\*)/g, '<em>$1</em>');

  const renderMarkdown = value => {
    const lines = String(value || '').replace(/\r\n/g, '\n').split('\n');
    const html = [];
    let list = null;
    let paragraph = [];
    let code = false;
    let codeLines = [];

    const flushParagraph = () => {
      if (!paragraph.length) return;
      html.push(`<p>${paragraph.map(inlineMarkdown).join('<br>')}</p>`);
      paragraph = [];
    };
    const closeList = () => {
      if (!list) return;
      html.push(`</${list}>`);
      list = null;
    };

    for (const line of lines) {
      if (line.trim().startsWith('```')) {
        flushParagraph(); closeList();
        if (code) { html.push(`<pre><code>${escapeHtml(codeLines.join('\n'))}</code></pre>`); codeLines = []; }
        code = !code;
        continue;
      }
      if (code) { codeLines.push(line); continue; }
      if (!line.trim()) { flushParagraph(); closeList(); continue; }

      if (/^\s{0,3}([-*_])(?:\s*\1){2,}\s*$/.test(line)) {
        flushParagraph(); closeList(); html.push('<hr>'); continue;
      }

      const heading = line.match(/^\s{0,3}(#{1,6})\s+(.+)$/);
      if (heading) {
        flushParagraph(); closeList();
        const level = Math.min(5, heading[1].length + 2);
        html.push(`<h${level}>${inlineMarkdown(heading[2].replace(/\s+#+\s*$/, ''))}</h${level}>`);
        continue;
      }

      const unordered = line.match(/^\s*[-*+]\s+(.+)$/);
      const ordered = line.match(/^\s*\d+[.)]\s+(.+)$/);
      if (unordered || ordered) {
        flushParagraph();
        const type = unordered ? 'ul' : 'ol';
        if (list !== type) { closeList(); html.push(`<${type}>`); list = type; }
        html.push(`<li>${inlineMarkdown((unordered || ordered)[1])}</li>`);
        continue;
      }

      closeList();
      paragraph.push(line);
    }
    if (codeLines.length) html.push(`<pre><code>${escapeHtml(codeLines.join('\n'))}</code></pre>`);
    flushParagraph(); closeList();
    return html.join('');
  };

  const resize = () => {
    input.style.height = 'auto';
    input.style.height = `${Math.min(input.scrollHeight, 150)}px`;
    counter.textContent = `${input.value.length} / 2000`;
  };

  const addMessage = (role, content, meta) => {
    messages.querySelector('.assistant-welcome')?.remove();
    const row = document.createElement('div');
    row.className = `chat-row ${role}`;
    const avatar = document.createElement('span');
    avatar.className = 'chat-avatar';
    avatar.textContent = role === 'assistant' ? '✦' : 'EU';
    const body = document.createElement('div');
    body.className = 'chat-body';
    const text = document.createElement('div');
    text.className = 'chat-content';
    if (role === 'assistant') text.innerHTML = renderMarkdown(content);
    else text.textContent = content;
    body.appendChild(text);
    if (meta) {
      const small = document.createElement('small');
      small.textContent = meta;
      body.appendChild(small);
    }
    row.append(avatar, body);
    messages.appendChild(row);
    messages.scrollTop = messages.scrollHeight;
    return row;
  };

  document.querySelectorAll('.assistant-suggestions button').forEach(button => button.addEventListener('click', () => {
    input.value = button.dataset.question || '';
    resize();
    input.focus();
  }));

  input.addEventListener('input', resize);
  input.addEventListener('keydown', event => {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      form.requestSubmit();
    }
  });

  form.addEventListener('submit', async event => {
    event.preventDefault();
    const question = input.value.trim();
    if (!question || submit.disabled) return;

    addMessage('user', question);
    input.value = '';
    resize();
    submit.disabled = true;
    input.disabled = true;
    const pending = addMessage('assistant', 'A consultar os dados do dossier…');
    pending.classList.add('pending');

    try {
      const response = await fetch('/AiAssistant/Ask', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
        body: JSON.stringify({ dossierId, question, history: history.slice(-12) })
      });
      const result = await response.json();
      pending.remove();
      if (!response.ok) throw new Error(result.error || 'Não foi possível concluir o pedido.');
      addMessage('assistant', result.answer, 'Resposta baseada nas tools disponíveis');
      history.push({ role: 'user', content: question }, { role: 'assistant', content: result.answer });
    } catch (error) {
      pending.remove();
      addMessage('assistant', error.message || 'Não foi possível concluir o pedido.', 'Erro ao processar a pergunta');
    } finally {
      submit.disabled = false;
      input.disabled = false;
      input.focus();
    }
  });

  resize();
})();
