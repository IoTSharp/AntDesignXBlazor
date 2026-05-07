function applyFilesToInput(input, files) {
  if (!input || !files) {
    return;
  }

  const dataTransfer = new DataTransfer();
  for (const file of files) {
    dataTransfer.items.add(file);
  }

  input.files = dataTransfer.files;
  input.dispatchEvent(new Event('change', { bubbles: true }));
}

export async function renderMermaid(element, source, options) {
  if (!element) {
    return;
  }

  element.textContent = source || '';

  if (!source || !window.mermaid) {
    element.classList.add('antdx-mermaid-fallback');
    return;
  }

  try {
    const theme = options?.theme || 'default';
    const config = options?.config || {};
    window.mermaid.initialize({ startOnLoad: false, securityLevel: 'strict', theme, ...config });
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

export function autoSizeTextarea(textarea, minRows = 2, maxRows = 8) {
  if (!textarea) {
    return;
  }

  textarea.style.height = 'auto';
  const computed = window.getComputedStyle(textarea);
  const lineHeight = Number.parseFloat(computed.lineHeight) || 20;
  const padding = (Number.parseFloat(computed.paddingTop) || 0) + (Number.parseFloat(computed.paddingBottom) || 0);
  const border = (Number.parseFloat(computed.borderTopWidth) || 0) + (Number.parseFloat(computed.borderBottomWidth) || 0);
  const minHeight = (lineHeight * minRows) + padding + border;
  const maxHeight = (lineHeight * maxRows) + padding + border;
  const nextHeight = Math.min(maxHeight, Math.max(minHeight, textarea.scrollHeight));

  textarea.style.height = `${nextHeight}px`;
  textarea.style.overflowY = textarea.scrollHeight > maxHeight ? 'auto' : 'hidden';
}

export function wireAttachmentDropZone(root, allowDrop, allowPaste, disabled) {
  if (!root) {
    return null;
  }

  const input = root.querySelector('input[type="file"]');
  if (!input) {
    return null;
  }

  const hoverClass = 'antdx-attachments-drag-over';
  const isDisabled = () => disabled || input.disabled || root.classList.contains('antdx-attachments-disabled');
  const toggleHover = (value) => {
    if (allowDrop && !isDisabled()) {
      root.classList.toggle(hoverClass, value);
    }
  };

  const onDragEnter = (event) => {
    if (!allowDrop || isDisabled()) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    toggleHover(true);
  };

  const onDragOver = (event) => {
    if (!allowDrop || isDisabled()) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    toggleHover(true);
  };

  const onDragLeave = (event) => {
    if (!allowDrop || isDisabled()) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();

    if (event.target === root) {
      toggleHover(false);
    }
  };

  const onDrop = (event) => {
    if (!allowDrop || isDisabled()) {
      return;
    }

    event.preventDefault();
    event.stopPropagation();
    toggleHover(false);

    const files = event.dataTransfer?.files;
    if (files?.length) {
      applyFilesToInput(input, files);
    }
  };

  const onPaste = (event) => {
    if (!allowPaste || isDisabled()) {
      return;
    }

    const files = event.clipboardData?.files;
    if (files?.length) {
      event.preventDefault();
      applyFilesToInput(input, files);
    }
  };

  root.addEventListener('dragenter', onDragEnter);
  root.addEventListener('dragover', onDragOver);
  root.addEventListener('dragleave', onDragLeave);
  root.addEventListener('drop', onDrop);
  root.addEventListener('paste', onPaste);

  return {
    dispose() {
      root.removeEventListener('dragenter', onDragEnter);
      root.removeEventListener('dragover', onDragOver);
      root.removeEventListener('dragleave', onDragLeave);
      root.removeEventListener('drop', onDrop);
      root.removeEventListener('paste', onPaste);
      root.classList.remove(hoverClass);
    },
  };
}

export function scrollToBottom(element) {
  if (element) {
    element.scrollTop = element.scrollHeight;
  }
}

export function focusElement(element, preventScroll) {
  if (!element || typeof element.focus !== 'function') {
    return;
  }
  try {
    element.focus({ preventScroll: !!preventScroll });
  } catch {
    element.focus();
  }
}

export function blurElement(element) {
  if (element && typeof element.blur === 'function') {
    element.blur();
  }
}

export function isSpeechSupported() {
  return !!(window.SpeechRecognition || window.webkitSpeechRecognition);
}

export function startSpeech(dotnetRef, options) {
  const Recognition = window.SpeechRecognition || window.webkitSpeechRecognition;
  if (!Recognition || !dotnetRef) {
    return null;
  }

  const recognition = new Recognition();
  recognition.lang = options?.lang || (navigator.language || 'en-US');
  recognition.continuous = !!options?.continuous;
  recognition.interimResults = options?.interimResults !== false;

  recognition.onresult = (event) => {
    let finalText = '';
    let interimText = '';
    for (let i = event.resultIndex; i < event.results.length; i++) {
      const result = event.results[i];
      if (result.isFinal) {
        finalText += result[0].transcript;
      } else {
        interimText += result[0].transcript;
      }
    }
    dotnetRef.invokeMethodAsync('OnSpeechResult', finalText, interimText);
  };

  recognition.onerror = (event) => {
    dotnetRef.invokeMethodAsync('OnSpeechError', event.error || 'unknown');
  };

  recognition.onend = () => {
    dotnetRef.invokeMethodAsync('OnSpeechEnd');
  };

  try {
    recognition.start();
  } catch (error) {
    dotnetRef.invokeMethodAsync('OnSpeechError', String(error?.message || error));
    return null;
  }

  return {
    stop() {
      try { recognition.stop(); } catch { /* noop */ }
    },
  };
}
