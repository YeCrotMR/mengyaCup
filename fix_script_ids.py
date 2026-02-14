import json
import os
import re

# Configuration
BASE_DIR = r"e:\Git\mengyaCup\Assets\StreamingAssets"
CHAPTER_FILES = {
    "Act01_Chapter01_Trial.json": 0,
    "Act01_Chapter02_Trial.json": 1,
    "Act01_Chapter03_Trial.json": 2,
    "Act01_Chapter04_Trial.json": 3
}

def reindex_chapter(file_path, chapter_idx):
    if not os.path.exists(file_path):
        print(f"Skipping (not found): {file_path}")
        return

    print(f"Processing: {os.path.basename(file_path)} (Index: {chapter_idx})")
    
    with open(file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    old_to_new = {}
    new_lines = []
    
    dialogue_count = 0
    cmd_counts = {}

    # Pass 1: Assign new IDs
    for item in data.get('lines', []):
        old_id = item.get('id', '')
        type_ = item.get('type', '')
        
        new_id = old_id # Default fallback

        if type_ == 'dialogue':
            new_id = f"line_{chapter_idx}_{dialogue_count}"
            dialogue_count += 1
        elif type_ == 'command':
            cmd_type = item.get('command', 'Unknown')
            if cmd_type not in cmd_counts:
                cmd_counts[cmd_type] = 0
            new_id = f"cmd_{cmd_type}_{cmd_counts[cmd_type]}"
            cmd_counts[cmd_type] += 1
        
        # Store mapping even if ID didn't change (though it likely will)
        old_to_new[old_id] = new_id
        item['id'] = new_id
        new_lines.append(item)

    # Pass 2: Update references in parameters
    updated_count = 0
    for item in new_lines:
        if item['type'] == 'command':
            cmd = item.get('command')
            params = item.get('parameters', [])
            original_params = str(params) # For comparison
            
            if cmd == 'Jump':
                # Param 0 is target ID
                if params and len(params) > 0:
                    target = params[0]
                    if target in old_to_new:
                        params[0] = old_to_new[target]

            elif cmd == 'ShowChoice':
                # Params are "Text|TargetID"
                new_params = []
                for p in params:
                    if '|' in p:
                        parts = p.split('|')
                        text = parts[0]
                        target = parts[1] if len(parts) > 1 else ""
                        
                        # Handle potential whitespace in target
                        target = target.strip()
                        
                        if target in old_to_new:
                            target = old_to_new[target]
                        
                        new_params.append(f"{text}|{target}")
                    else:
                        new_params.append(p)
                item['parameters'] = new_params
                
            elif cmd == 'RequireEvidence':
                # [Name, SuccessID, FailID]
                if len(params) >= 3:
                    if params[1] in old_to_new:
                        params[1] = old_to_new[params[1]]
                    if params[2] in old_to_new:
                        params[2] = old_to_new[params[2]]
            
            # Update the item with new parameters if they exist or were created
            if 'parameters' in item or params:
                 item['parameters'] = params

            if str(params) != original_params:
                updated_count += 1

    data['lines'] = new_lines

    with open(file_path, 'w', encoding='utf-8') as f:
        json.dump(data, f, ensure_ascii=False, indent=4)
    
    print(f"  - Reindexed {len(new_lines)} lines.")
    print(f"  - Updated references in {updated_count} commands.")
    print("  - Done.")

def main():
    print("Starting Script ID Fixer...")
    for filename, idx in CHAPTER_FILES.items():
        full_path = os.path.join(BASE_DIR, filename)
        reindex_chapter(full_path, idx)
    print("All operations completed.")

if __name__ == "__main__":
    main()
