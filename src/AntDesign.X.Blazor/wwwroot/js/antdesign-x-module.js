export async function renderMermaid(element, source) {
  if (!element) {
    return;
  }

  element.textContent = source || '';

  if (!source || !window.mermaid) {
    element.classList.add('antdx-mermaid-fallback');
    return;
  }

  try {
    window.mermaid.initialize({ startOnLoad: false, securityLevel: 'strict' });
    const id = `antdx-mermaid-${Math.random().toString(36).slice(2)}`;
    const result = await window.mermaid.render(id, source);
    element.innerHTML = result.svg;
    element.classList.remove('antdx-mermaid-fallback');
  } catch (error) {
    element.textContent = source;
    element.classList.add('antdx-mermaid-fallback');
    console.warn('AntDesign.X.Blazor Mermaid render failed', error);
  }
}

export function scrollToBottom(element) {
  if (element) {
    element.scrollTop = element.scrollHeight;
  }
}
