import { useEffect, useId, useRef, useState } from 'react';

export interface SearchableSelectOption {
  value: string;
  label: string;
  group?: string;
  searchText?: string;
}

interface SearchableSelectProps {
  options: SearchableSelectOption[];
  value: string;
  onChange: (value: string) => void;
  placeholder?: string;
}

export function SearchableSelect({
  options,
  value,
  onChange,
  placeholder = 'Select...',
}: SearchableSelectProps) {
  const listboxId = useId();
  const containerRef = useRef<HTMLDivElement>(null);
  const inputRef = useRef<HTMLInputElement>(null);
  const [open, setOpen] = useState(false);
  const [query, setQuery] = useState('');

  const selected = options.find((o) => o.value === value);

  const normalizedQuery = query.trim().toLowerCase();
  const filtered = normalizedQuery
    ? options.filter((o) => {
        const haystack = [o.label, o.group, o.searchText].filter(Boolean).join(' ').toLowerCase();
        return haystack.includes(normalizedQuery);
      })
    : options;

  const grouped = filtered.reduce<Record<string, SearchableSelectOption[]>>((acc, option) => {
    const group = option.group || 'Other';
    (acc[group] ??= []).push(option);
    return acc;
  }, {});

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false);
        setQuery('');
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const selectOption = (option: SearchableSelectOption) => {
    onChange(option.value);
    setOpen(false);
    setQuery('');
    inputRef.current?.blur();
  };

  const displayValue = open ? query : (selected?.label ?? '');

  return (
    <div className="searchable-select" ref={containerRef}>
      <input
        ref={inputRef}
        type="text"
        role="combobox"
        aria-expanded={open}
        aria-controls={listboxId}
        aria-autocomplete="list"
        value={displayValue}
        placeholder={selected ? undefined : placeholder}
        onChange={(e) => {
          setQuery(e.target.value);
          if (!open) setOpen(true);
        }}
        onFocus={() => {
          setOpen(true);
          setQuery('');
        }}
        onKeyDown={(e) => {
          if (e.key === 'Escape') {
            setOpen(false);
            setQuery('');
            inputRef.current?.blur();
          } else if (e.key === 'Enter' && filtered.length === 1) {
            e.preventDefault();
            selectOption(filtered[0]);
          }
        }}
      />
      {open && (
        <ul className="searchable-select-dropdown" id={listboxId} role="listbox">
          {filtered.length === 0 ? (
            <li className="searchable-select-empty">No products found</li>
          ) : (
            Object.entries(grouped).map(([group, items]) => (
              <li key={group} className="searchable-select-group" role="presentation">
                <span className="searchable-select-group-label">{group}</span>
                <ul role="group" aria-label={group}>
                  {items.map((option) => (
                    <li
                      key={option.value}
                      role="option"
                      aria-selected={option.value === value}
                      className={option.value === value ? 'selected' : undefined}
                      onMouseDown={(e) => {
                        e.preventDefault();
                        selectOption(option);
                      }}
                    >
                      {option.label}
                    </li>
                  ))}
                </ul>
              </li>
            ))
          )}
        </ul>
      )}
    </div>
  );
}
