import { Download, ExternalLink, FileText, Trash2, Upload } from 'lucide-react';
import React, { useState } from 'react';

interface FileItem {
  id: string;
  fileName: string;
  originalFileName: string;
  size: number;
  mimeType: string;
  createdAt: string;
  publicUrl?: string;
  isPublic: boolean;
}

interface FileUploadResult {
  fileId: string;
  fileName: string;
  publicUrl?: string;
  size: number;
  success: boolean;
  errorMessage?: string;
}

const FileUploadDemo: React.FC = () => {
  const [files, setFiles] = useState<FileItem[]>([]);
  const [uploading, setUploading] = useState(false);
  const [dragOver, setDragOver] = useState(false);

  const formatFileSize = (bytes: number): string => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  const formatDate = (dateString: string): string => {
    return new Date(dateString).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    });
  };

  const uploadFile = async (file: File, isPublic: boolean = false): Promise<FileUploadResult> => {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('isPublic', isPublic.toString());

    const response = await fetch('/api/files/upload', {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || 'Upload failed');
    }

    return await response.json();
  };

  const fetchFiles = async (): Promise<void> => {
    try {
      const response = await fetch('/api/files?pageSize=50');
      if (response.ok) {
        const data = await response.json();
        setFiles(data.files || []);
      }
    } catch (error) {
      console.error('Error fetching files:', error);
    }
  };

  const handleFileUpload = async (selectedFiles: FileList, isPublic: boolean = false): Promise<void> => {
    setUploading(true);
    try {
      const uploadPromises = Array.from(selectedFiles).map(file => uploadFile(file, isPublic));
      const results = await Promise.all(uploadPromises);

      const successCount = results.filter(r => r.success).length;
      if (successCount > 0) {
        await fetchFiles(); // Refresh file list
      }

      const failedCount = results.filter(r => !r.success).length;
      if (failedCount > 0) {
        alert(`${failedCount} file(s) failed to upload`);
      }
    } catch (error) {
      console.error('Upload error:', error);
      alert('Upload failed: ' + (error as Error).message);
    } finally {
      setUploading(false);
    }
  };

  const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>): void => {
    const selectedFiles = event.target.files;
    if (selectedFiles && selectedFiles.length > 0) {
      handleFileUpload(selectedFiles);
    }
  };

  const handleDrop = (event: React.DragEvent<HTMLDivElement>): void => {
    event.preventDefault();
    setDragOver(false);

    const droppedFiles = event.dataTransfer.files;
    if (droppedFiles.length > 0) {
      handleFileUpload(droppedFiles);
    }
  };

  const handleDragOver = (event: React.DragEvent<HTMLDivElement>): void => {
    event.preventDefault();
    setDragOver(true);
  };

  const handleDragLeave = (): void => {
    setDragOver(false);
  };

  const downloadFile = async (fileId: string, fileName: string): Promise<void> => {
    try {
      const response = await fetch(`/api/files/${fileId}/download`);
      if (response.ok) {
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
      }
    } catch (error) {
      console.error('Download error:', error);
      alert('Download failed');
    }
  };

  const deleteFile = async (fileId: string): Promise<void> => {
    if (confirm('Are you sure you want to delete this file?')) {
      try {
        const response = await fetch(`/api/files/${fileId}`, {
          method: 'DELETE',
        });
        if (response.ok) {
          await fetchFiles(); // Refresh file list
        }
      } catch (error) {
        console.error('Delete error:', error);
        alert('Delete failed');
      }
    }
  };

  React.useEffect(() => {
    fetchFiles();
  }, []);

  return (
    <div className="max-w-6xl mx-auto p-6">
      <h1 className="text-3xl font-bold text-gray-800 mb-2">
        <span className="text-blue-500">GCP</span> File Upload
      </h1>
      <p className="text-gray-600 mb-8">
        This is an example file upload page using GCP Storage. Maybe your app
        needs this. Maybe it doesn't. But a lot of people asked for this
        feature, so here you go 🔥
      </p>

      {/* Upload Area */}
      <div
        className={`border-2 border-dashed rounded-lg p-8 text-center mb-8 transition-colors ${
          dragOver
            ? 'border-blue-500 bg-blue-50'
            : 'border-gray-300 hover:border-gray-400'
        }`}
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onDragLeave={handleDragLeave}
      >
        <Upload className="mx-auto h-12 w-12 text-gray-400 mb-4" />
        <p className="text-lg text-gray-600 mb-2">
          Drop files here or click to browse
        </p>
        <input
          type="file"
          multiple
          onChange={handleFileChange}
          className="hidden"
          id="fileInput"
          disabled={uploading}
        />
        <label
          htmlFor="fileInput"
          className={`inline-block px-6 py-3 bg-blue-500 text-white rounded-lg cursor-pointer hover:bg-blue-600 transition-colors ${
            uploading ? 'opacity-50 cursor-not-allowed' : ''
          }`}
        >
          {uploading ? 'Uploading...' : 'Choose Files'}
        </label>
      </div>

      {/* Files List */}
      <div className="bg-white rounded-lg shadow">
        <div className="px-6 py-4 border-b border-gray-200">
          <h2 className="text-xl font-semibold text-gray-800">Uploaded Files</h2>
          {files.length === 0 && (
            <p className="text-gray-500 mt-2">No files uploaded yet :(</p>
          )}
        </div>

        {files.length > 0 && (
          <div className="divide-y divide-gray-200">
            {files.map((file) => (
              <div key={file.id} className="px-6 py-4 flex items-center justify-between">
                <div className="flex items-center space-x-4">
                  <FileText className="h-8 w-8 text-blue-500" />
                  <div>
                    <h3 className="font-medium text-gray-900">{file.originalFileName}</h3>
                    <p className="text-sm text-gray-500">
                      {formatFileSize(file.size)} • {formatDate(file.createdAt)}
                      {file.isPublic && (
                        <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800">
                          Public
                        </span>
                      )}
                    </p>
                  </div>
                </div>

                <div className="flex items-center space-x-2">
                  {file.publicUrl && (
                    <a
                      href={file.publicUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="p-2 text-gray-500 hover:text-blue-500 transition-colors"
                      title="Open in new tab"
                    >
                      <ExternalLink className="h-4 w-4" />
                    </a>
                  )}
                  <button
                    onClick={() => downloadFile(file.id, file.originalFileName)}
                    className="p-2 text-gray-500 hover:text-blue-500 transition-colors"
                    title="Download"
                  >
                    <Download className="h-4 w-4" />
                  </button>
                  <button
                    onClick={() => deleteFile(file.id)}
                    className="p-2 text-gray-500 hover:text-red-500 transition-colors"
                    title="Delete"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default FileUploadDemo;
