(() => {
  const escapeHtml = value => value.replace(/[&<>"']/g, character => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[character]));
  const inlineMarkdown = value => escapeHtml(value)
    .replace(/`([^`]+)`/g, '<code>$1</code>')
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
    .replace(/__([^_]+)__/g, '<strong>$1</strong>')
    .replace(/(?<!\*)\*([^*\n]+)\*(?!\*)/g, '<em>$1</em>');

  const renderMarkdown = value => {
    const lines = String(value || '').replace(/\r\n/g, '\n').replace(/\r/g, '\n').split('\n');
    const html = [];
    let list = null;
    let paragraph = [];
    let code = false;
    let codeLines = [];
    const flushParagraph = () => { if (!paragraph.length) return; html.push(`<p>${paragraph.map(inlineMarkdown).join('<br>')}</p>`); paragraph = []; };
    const closeList = () => { if (!list) return; html.push(`</${list}>`); list = null; };

    for (const line of lines) {
      if (line.trim().startsWith('```')) { flushParagraph(); closeList(); if (code) { html.push(`<pre><code>${escapeHtml(codeLines.join('\n'))}</code></pre>`); codeLines = []; } code = !code; continue; }
      if (code) { codeLines.push(line); continue; }
      if (!line.trim()) { flushParagraph(); closeList(); continue; }
      if (/^\s{0,3}([-*_])(?:\s*\1){2,}\s*$/.test(line)) { flushParagraph(); closeList(); html.push('<hr>'); continue; }
      const heading = line.match(/^\s{0,3}(#{1,6})\s+(.+)$/);
      if (heading) { flushParagraph(); closeList(); const level = Math.min(5, heading[1].length + 2); html.push(`<h${level}>${inlineMarkdown(heading[2].replace(/\s+#+\s*$/, ''))}</h${level}>`); continue; }
      const unordered = line.match(/^\s*[-*+]\s+(.+)$/);
      const ordered = line.match(/^\s*\d+[.)]\s+(.+)$/);
      if (unordered || ordered) { flushParagraph(); const type = unordered ? 'ul' : 'ol'; if (list !== type) { closeList(); html.push(`<${type}>`); list = type; } html.push(`<li>${inlineMarkdown((unordered || ordered)[1])}</li>`); continue; }
      closeList(); paragraph.push(line);
    }
    if (codeLines.length) html.push(`<pre><code>${escapeHtml(codeLines.join('\n'))}</code></pre>`);
    flushParagraph(); closeList(); return html.join('');
  };

  document.querySelectorAll('[data-markdown-source]').forEach(element => { element.innerHTML = renderMarkdown(element.textContent || ''); });
})();
